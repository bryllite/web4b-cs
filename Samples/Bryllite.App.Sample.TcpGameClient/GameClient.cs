using Bryllite.App.Sample.Common;
using Bryllite.Extensions;
using Bryllite.Rpc.Web4b;
using Bryllite.Utils.NabiLog;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.App.Sample.TcpGameClient
{
    public class GameClient
    {
        private readonly GameClientApplication app;

        // connection
        private TcpConnection connection;

        // is connected?
        public bool Connected => connection.Connected;

        // uid
        public string Uid { get; private set; }

        // session key
        public string SessionKey { get; private set; }

        // user address
        public string UserAddress { get; private set; }


        // poa helper
        private PoAHelper poa;

        // cyprus api
        private CyprusApi cyprus;

        public GameClient(GameClientApplication app)
        {
            this.app = app;

            // tcp game connection
            connection = new TcpConnection()
            {
                OnConnected = OnConnected,
                OnDisconnected = OnDisconnected,
                OnMessage = OnMessage,
                OnWritten = OnWritten
            };

            // map message handler
            MapMessageHandler("error", OnMessageError);
            MapMessageHandler("login.res", OnMessageLoginRes);
            MapMessageHandler("logout.res", OnMessageLogoutRes);
            MapMessageHandler("token.res", OnMessageTokenRes);
            MapMessageHandler("info.res", OnMessageInfoRes);
            MapMessageHandler("transfer.res", OnMessageTransferRes);
            MapMessageHandler("payout.res", OnMessagePayoutRes);

            MapMessageHandler("shop.list.res", OnMessageShopListRes);
            MapMessageHandler("shop.buy.res", OnMessageShopBuyRes);

            MapMessageHandler("market.register.res", OnMessageMarketRegisterRes);
            MapMessageHandler("market.unregister.res", OnMessageMarketUnregisterRes);
            MapMessageHandler("market.list.res", OnMessageMarketListRes);
            MapMessageHandler("market.buy.res", OnMessageMarketBuyRes);

            // cyprus api
            cyprus = new CyprusApi(app.BridgeUrl);
        }

        public void Start(string remote)
        {
            connection.Start(remote);
        }

        public void Stop()
        {
            // stop poa helper
            poa?.Stop(CancellationToken.None);

            connection.Stop();
        }

        public bool Send(byte[] message)
        {
            return connection.Send(message) > 0;
        }


        public void Login(string uid, string passcode)
        {
            // uid 저장
            Uid = uid;

            // send login message
            Send(
                new GameMessage("login.req")
                .With("uid", uid)
                .With("passcode", passcode)
            );
        }

        public void Logout()
        {
            Send(
                new GameMessage("logout.req")
                .With("session", SessionKey)
            );
        }

        public void Info()
        {
            Send(
                new GameMessage("info.req")
                .With("session", SessionKey)
            );
        }

        public void Transfer(string to, decimal value)
        {
            Send(new GameMessage("transfer.req").With("session", SessionKey).With("to", to).With("value", value));
        }

        public void Payout(string to, decimal value)
        {
            Send(new GameMessage("payout.req").With("session", SessionKey).With("to", to).With("value", value));
        }

        public void ShopList()
        {
            Send(new GameMessage("shop.list.req").With("session", SessionKey));
        }

        public void ShopBuy(string itemcode)
        {
            Send(new GameMessage("shop.buy.req").With("session", SessionKey).With("itemcode", itemcode));
        }

        public void MarketRegister(string itemcode, decimal price)
        {
            Send(new GameMessage("market.register.req").With("session", SessionKey).With("itemcode", itemcode).With("price", price));
        }

        public void MarketUnregister(string order)
        {
            Send(new GameMessage("market.unregister.req").With("session", SessionKey).With("order", order));
        }

        public void MarketList()
        {
            Send(new GameMessage("market.list.req").With("session", SessionKey));
        }

        public void MarketBuy(string order)
        {
            Send(new GameMessage("market.buy.req").With("session", SessionKey).With("order", order));
        }


        public async Task<ulong> GetBalanceAsync()
        {
            return await cyprus.GetBalanceAsync(UserAddress, "latest") ?? 0;
        }

        private void OnConnected(TcpSession session, bool connected)
        {
            Log.Info("OnConnected! connected=", connected);
        }

        private void OnDisconnected(TcpSession session, int reason)
        {
            Log.Info("OnDisonnected! reason=", reason);
        }

        private void OnWritten(TcpSession session, byte[] message)
        {
            //Log.Info("OnWritten! message=", message.Length);
        }

        // message handler type
        public delegate void GameMessageHandler(TcpSession session, GameMessage message);

        // message handler table
        private Dictionary<string, GameMessageHandler> handlers = new Dictionary<string, GameMessageHandler>();

        private GameMessageHandler GetMessageHandler(string messageId)
        {
            string id = messageId.ToLower();
            lock (handlers)
                return handlers.ContainsKey(id) ? handlers[id] : null;
        }

        private void MapMessageHandler(string messageId, GameMessageHandler handler)
        {
            string id = messageId.ToLower();
            lock (handlers)
                handlers[id] = handler;
        }

        private void OnMessage(TcpSession session, byte[] received)
        {
            // parse message
            if (!GameMessage.TryParse(received, out GameMessage message))
            {
                Log.Warning("can't parse game message! received=", Hex.ToString(received.Length));
                return;
            }

            // get message handler
            var handler = GetMessageHandler(message.MessageId);
            if (null == handler)
            {
                Log.Warning("unknown message! message.id=", message.MessageId);
                return;
            }

            // invoke message handler
            handler.Invoke(session, message);
        }

        private void OnMessageError(TcpSession session, GameMessage message)
        {
            BConsole.WriteLine("error=", message);
        }

        private void OnMessageLoginRes(TcpSession session, GameMessage message)
        {
            // session code & user address
            SessionKey = message.Get<string>("scode");
            UserAddress = message.Get<string>("address");

            // start poa helper
            poa = new PoAHelper(new Uri(app.PoAUrl), UserAddress, OnPoARequest);
            poa.Start(CancellationToken.None);

            BConsole.WriteLine("login success! session=", SessionKey, ", address=", UserAddress);
        }

        private void OnMessageLogoutRes(TcpSession session, GameMessage message)
        {
            // 연결을 종료한다
            Stop();

            BConsole.WriteLine("logout success!");
        }

        private void OnMessageTokenRes(TcpSession session, GameMessage message)
        {
            poaToken = message.Get<string>("accessToken");
        }

        private string poaToken;

        private string OnPoARequest(string hash, string iv)
        {
            poaToken = null;

            // game server에 access token을 요청한다.
            Send(
                new GameMessage("token.req")
                .With("session", SessionKey)
                .With("hash", hash)
                .With("iv", iv)
            );

            // 최대 5초 동안 accessToken 수신 대기한다
            var sw = Stopwatch.StartNew();
            while (string.IsNullOrEmpty(poaToken) && sw.ElapsedMilliseconds < 5000)
                Thread.Sleep(10);

            Log.Debug("accessToken=", poaToken);
            return poaToken;
        }


        private void OnMessageInfoRes(TcpSession session, GameMessage message)
        {
            // info
            BConsole.WriteLine(message.Get<JObject>("info"));
        }

        private void OnMessageTransferRes(TcpSession session, GameMessage message)
        {
            // txid
            string txid = message.Get<string>("txid");
            BConsole.WriteLine("txid=", txid);
        }

        private void OnMessagePayoutRes(TcpSession session, GameMessage message)
        {
            // txid
            string txid = message.Get<string>("txid");
            BConsole.WriteLine("txid=", txid);
        }

        private void OnMessageShopListRes(TcpSession session, GameMessage message)
        {
            // items
            var items = message.Get<JArray>("items");
            BConsole.WriteLine("shop=", items);
        }

        private void OnMessageShopBuyRes(TcpSession session, GameMessage message)
        {
            // txid
            var item = message.Get<JObject>("item");
            BConsole.WriteLine("item=", item);
        }

        private void OnMessageMarketRegisterRes(TcpSession session, GameMessage message)
        {
            string order = message.Get<string>("order");
            BConsole.WriteLine("market registered! order=", order);
        }

        private void OnMessageMarketUnregisterRes(TcpSession session, GameMessage message)
        {
            string order = message.Get<string>("order");
            BConsole.WriteLine("market unregistered! order=", order);
        }

        private void OnMessageMarketListRes(TcpSession session, GameMessage message)
        {
            var sales = message.Get<JArray>("sales");
            if (sales.Count == 0)
            {
                BConsole.WriteLine("no sales in market");
                return;
            }

            foreach (var sale in sales)
                BConsole.WriteLine(sale);
        }

        private void OnMessageMarketBuyRes(TcpSession session, GameMessage message)
        {
            string name = message.Get<string>("itemname");
            decimal price = message.Get<decimal>("price");

            BConsole.WriteLine("market trade! item=", name, ", price=", price);
        }


    }
}
