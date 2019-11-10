using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bryllite.Rpc.Web4b.Providers
{
    public interface IWeb4bProvider
    {
        Task<string> PostAsync(string body, CancellationToken cancellation);
        Task<string> PostAsync(string body);
    }
}
