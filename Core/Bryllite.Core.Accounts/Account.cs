using Bryllite.Extensions;
using Bryllite.Utils.Currency;
using Bryllite.Utils.Rlp;
using Newtonsoft.Json.Linq;
using System;

namespace Bryllite.Core.Accounts
{
    public class Account
    {
        // network id
        protected Hex chain = 0x00;
        public byte Chain
        {
            get { return chain; }
            set { chain = value; Dirty = true; }
        }

        // version
        protected Hex version = 0x00;
        public byte Version
        {
            get { return version; }
            set { version = value; Dirty = true; }
        }

        // balance
        protected Hex balance = 0x00;
        public ulong Balance
        {
            get { return balance; }
            set { balance = value; Dirty = true; }
        }

        // nonce
        protected Hex nonce = 0x00;
        public ulong Nonce
        {
            get { return nonce; }
            set { nonce = value; Dirty = true; }
        }

        // storage
        protected Hex storage = null;
        public byte[] Storage
        {
            get { return storage; }
            set { storage = value; Dirty = true; }
        }

        // contract
        protected Hex contract = null;
        public byte[] Contract
        {
            get { return contract; }
            set { contract = value; Dirty = true; }
        }

        // reserved
        protected Hex reserved = null;
        public byte[] Reserved
        {
            get { return reserved; }
            set { reserved = value; Dirty = true; }
        }

        // account rlp
        public byte[] Rlp => ToRlp();

        // account has changed?
        public bool Dirty = false;


        public Account()
        {
        }

        protected Account(byte[] rlp)
        {
            var decoder = new RlpDecoder(rlp);

            chain = decoder.Next();
            version = decoder.Next();
            balance = decoder.Next();
            nonce = decoder.Next();
            storage = decoder.Next();
            contract = decoder.Next();
            reserved = decoder.Next();
        }

        protected Account(string rlp) : this(Hex.ToByteArray(rlp))
        {
        }

        public byte[] ToRlp()
        {
            return RlpEncoder.EncodeList(chain, version, balance, nonce, storage, contract, reserved);
        }

        public JObject ToJObject()
        {
            var json = new JObject();

            json.Put("balance", Coin.ToCoin(Balance).ToString("N"));
            json.Put("nonce", Nonce);
            json.Put("storage", Hex.ToString(Storage));
            json.Put("contract", Hex.ToString(Contract));
            json.Put("reserved", Hex.ToString(Reserved));

            return json;
        }

        public static Account Parse(byte[] rlp)
        {
            return new Account(rlp);
        }

        public static Account Parse(string rlp)
        {
            return new Account(rlp);
        }

        public static bool TryParse(byte[] rlp, out Account account)
        {
            try
            {
                account = Parse(rlp);
                return true;
            }
            catch
            {
                account = null;
                return false;
            }
        }

        public static bool TryParse(string rlp, out Account account)
        {
            try
            {
                account = Parse(rlp);
                return true;
            }
            catch
            {
                account = null;
                return false;
            }
        }
    }
}
