using Bryllite.Utils.NabiLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Providers
{
    public class HttpProvider : IWeb4bProvider
    {
        public const string CONTENT_TYPE_JSON = "application/json";

        // remote url
        private Uri remote;

        public HttpProvider(Uri remote)
        {
            this.remote = remote;
        }

        public HttpProvider(string remote) : this(new Uri(remote))
        {
        }

        public async Task<string> PostAsync(string body, CancellationToken cancellation)
        {
            using (var connection = new HttpClient())
            {
                try
                {
                    // json string to contents
                    var contents = new StringContent(body, Encoding.UTF8, CONTENT_TYPE_JSON);

                    // post async
                    var response = await connection.PostAsync(remote, contents, cancellation);

                    // response
                    return await response.Content?.ReadAsStringAsync();
                }
                catch (HttpRequestException hrex)
                {
                    Log.Warning("HttpRequestException! hrex.Message=", hrex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Warning("exception! ex.Message=", ex.Message);
                    throw;
                }
            }
        }

        public async Task<string> PostAsync(string body)
        {
            return await PostAsync(body, CancellationToken.None);
        }
    }
}
