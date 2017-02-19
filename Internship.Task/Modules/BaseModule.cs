using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpServerCore;

namespace Internship.Modules
{
    public abstract class BaseModule : IServerModule
    {
        private IEnumerable<RequestFilter> filters { get; set; }
        protected abstract IEnumerable<RequestFilter> Filters { get; }

        public async Task<IRequest> ProcessRequest(IRequest request)
        {
            Console.WriteLine($"Get request: {request.Url.LocalPath}");
            if (filters == null)
                filters = Filters;
            return await filters.Aggregate(Task.FromResult(request),
                async (task, filter) => await filter.FilterRequest(await task)
            );
        }
    }
}
