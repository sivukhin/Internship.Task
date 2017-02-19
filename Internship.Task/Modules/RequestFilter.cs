using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;

namespace StatisticServer.Modules
{
    public class RequestFilter
    {
        private HttpMethodEnum AllowedMethods { get; set; }
        private Regex UrlPattern { get; set; }
        private Func<IRequest, Match, Task<IResponse>> RequestTransform { get; set; }

        public RequestFilter(HttpMethodEnum allowedMethods, Regex urlPattern, Func<IRequest, Task<IResponse>> requestTransform)
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
            if (!AllowedMethods.HasFlag(request.HttpMethod))
                return await Task.FromResult(request);
            var match = request.MatchLocalPath(UrlPattern);
            if (match.Success)
            {
                var response = await RequestTransform(request, match);
                return request.AttachResponse(response);
            }
            return await Task.FromResult(request);
        }
    }
}
