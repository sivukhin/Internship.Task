using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using NLog;

namespace Kontur.GameStats.Server.Modules
{
    public class RequestFilter
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public HttpMethodEnum AllowedMethods { get; }
        public Regex UrlPattern { get; }

        private Func<IRequest, Match, Task<IResponse>> RequestTransform { get; }

        public RequestFilter(HttpMethodEnum allowedMethods, Regex urlPattern,
            Func<IRequest, Task<IResponse>> requestTransform)
        {
            AllowedMethods = allowedMethods;
            UrlPattern = urlPattern;
            RequestTransform = (request, _) => requestTransform(request);
        }

        public RequestFilter(HttpMethodEnum allowedMethods, Regex urlPattern,
            Func<IRequest, Match, Task<IResponse>> requestTransform)
        {
            AllowedMethods = allowedMethods;
            UrlPattern = urlPattern;
            RequestTransform = requestTransform;
        }

        public async Task<IRequest> FilterRequest(IRequest request)
        {
            logger.ConditionalTrace("Process request: {0}", new {Filter = this, Request = request});

            if (AllowedMethods != request.HttpMethod)
                return await Task.FromResult(request);
            var match = request.MatchLocalPath(UrlPattern);
            if (match.Success)
            {
                logger.ConditionalTrace("Accept request: {0}", new {Filter = this, Request = request});
                var response = await RequestTransform(request, match);
                logger.ConditionalTrace("Generate response: {0}", new {Request = request, Response = response});
                return request.AttachResponse(response);
            }
            return await Task.FromResult(request);
        }

        public override string ToString()
        {
            return $"Filter({AllowedMethods}, {UrlPattern})";
        }
    }
}