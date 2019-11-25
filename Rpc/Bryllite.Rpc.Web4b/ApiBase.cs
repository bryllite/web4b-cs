using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class ApiBase
    {
        public static readonly string EARLIEST = "earliest";
        public static readonly string LATEST = "latest";
        public static readonly string PENDING = "pending";

        // api provider
        private IWeb4bProvider web4b;

        // errors
        private List<string> errors = new List<string>();

        public ApiBase(IWeb4bProvider web4b)
        {
            this.web4b = web4b;
        }

        public ApiBase(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                web4b = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                web4b = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public ApiBase(string remote) : this(new Uri(remote))
        {
        }

        public string GetLastError()
        {
            lock (errors)
                return errors.Last();
        }

        public IEnumerable<string> Errors()
        {
            lock (errors)
                return errors.ToArray();
        }

        public void SetLastError(string error)
        {
            Log.Warning("error: ", error);

            lock (errors)
                errors.Add(error);
        }


        // jsonrpc request async
        public async Task<JsonRpc> PostAsync(JsonRpc request)
        {
            return JsonRpc.TryParse(await web4b.PostAsync(request.ToString()), out JsonRpc response) ? response : null;
        }

        // jsonrpc batch request
        public async Task<JsonRpc.Batch> PostAsync(JsonRpc.Batch batch)
        {
            return JsonRpc.Batch.TryParse(await web4b.PostAsync(batch.ToString()), out JsonRpc.Batch response) ? response : null;
        }


        public async Task<string> GetVersionAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_getVersion", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }


        public async Task<long?> GetTimeAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_getTime", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return Hex.ToNumber<long>(response.Result<string>(0));

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<string> GetSha3Async(string message, int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_sha3", id, message));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<string>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }

        public async Task<bool?> GetMiningAsync(int id = 0)
        {
            try
            {
                var response = await PostAsync(new JsonRpc.Request("web4b_mining", id));
                if (!ReferenceEquals(response, null))
                {
                    if (!response.HasError)
                        return response.Result<bool>(0);

                    SetLastError(response.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                SetLastError(ex.ToString());
            }

            return null;
        }
    }
}
