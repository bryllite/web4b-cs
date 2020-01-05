using Bryllite.Rpc.Web4b.Providers;
using Bryllite.Utils.JsonRpc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b
{
    public class Web4b
    {
        public const string web4b_getVersion = "web4b_getVersion";
        public const string web4b_getTime = "web4b_getTime";
        public const string web4b_sha3 = "web4b_sha3";
        public const string web4b_mining = "web4b_mining";

        // api provider
        protected IWeb4bProvider provider;

        public Web4b(IWeb4bProvider provider)
        {
            this.provider = provider;
        }

        public Web4b(Uri remote)
        {
            if ("http" == remote.Scheme.ToLower())
                provider = new HttpProvider(remote);
            else if ("ws" == remote.Scheme.ToLower())
                provider = new WebSocketProvider(remote);
            else throw new ArgumentException("unsupported uri scheme");
        }

        public Web4b(string remote) : this(new Uri(remote))
        {
        }

        public async Task<(JsonRpc response, string error)> PostAsync(JsonRpc request)
        {
            return await PostAsync(request, CancellationToken.None);
        }

        public async Task<(JsonRpc response, string error)> PostAsync(JsonRpc request, CancellationToken cancellation)
        {
            try
            {
                if (JsonRpc.TryParse(await provider.PostAsync(request, cancellation), out JsonRpc response))
                {
                    if (response.HasError)
                        return (null, response.ErrorMessage);

                    return (response, null);
                }

                return(null, "response not parsable");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        public async Task<(JsonRpc.Batch response, string error)> PostAsync(JsonRpc.Batch requests)
        {
            return await PostAsync(requests, CancellationToken.None);
        }

        public async Task<(JsonRpc.Batch response, string error)> PostAsync(JsonRpc.Batch requests, CancellationToken cancellation)
        {
            try
            {
                return JsonRpc.Batch.TryParse(await provider.PostAsync(requests, cancellation), out JsonRpc.Batch responses) ? (responses, string.Empty) : (null, "response not parsable");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// get node versions tring
        /// method: web4b_getVersion
        /// params: none
        /// result: version string ( ex: web4b/0.5.5 )
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(string version, string error)> GetVersionAsync(int id = 0)
        {
            (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(web4b_getVersion, id));
            if (ReferenceEquals(response, null))
                return (string.Empty, error);

            try
            {
                return response.HasError ? (string.Empty, response.ErrorMessage) : (response.Result<string>(0), string.Empty);
            }
            catch (Exception ex)
            {
                return (string.Empty, ex.Message);
            }
        }

        /// <summary>
        /// get node time
        /// method: web4b_getTime
        /// params: none
        /// result: node unix time in milliseconds ( UTC )
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(long unixtime, string error)> GetTimeAsync(int id = 0)
        {
            (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(web4b_getTime, id));
            if (ReferenceEquals(response, null))
                return (0, error);

            try
            {
                return response.HasError ? (0, response.ErrorMessage) : (Hex.ToNumber<long>(response.Result<string>(0)), string.Empty);
            }
            catch (Exception ex)
            {
                return (0, ex.Message);
            }
        }

        /// <summary>
        /// get sha3 hash ( keccak )
        /// method: web4b_sha3
        /// params: message for sha3 ( message should be hex string or encode with utf8 )
        /// result: message hash hex string (32 bytes, 64 string)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(H256 hash, string error)> GetSha3Async(string message, int id = 0)
        {
            (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(web4b_sha3, id, Hex.IsHexString(message) ? message : Hex.ToString(Encoding.UTF8.GetBytes(message))));
            if (ReferenceEquals(response, null))
                return (null, error);

            try
            {
                return response.HasError ? (null, response.ErrorMessage) : (response.Result<string>(0), string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// mining?
        /// method: web4b_mining
        /// params: none
        /// result: mining?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<(bool? mining, string error)> GetMiningAsync(int id = 0)
        {
            (JsonRpc response, string error) = await PostAsync(new JsonRpc.Request(web4b_mining, id));
            if (ReferenceEquals(response, null))
                return (null, error);

            try
            {
                return response.HasError ? ((bool?)null, response.ErrorMessage) : (response.Result<bool>(0), string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}
