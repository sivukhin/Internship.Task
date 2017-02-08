using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Internship
{
    [AttributeUsage(AttributeTargets.All)]
    public class RequestFilterAttribute : Attribute
    {
        private static readonly Dictionary<string, HttpMethodEnum> httpMethodMap =
            new Dictionary<string, HttpMethodEnum>
            {
                ["get"] = HttpMethodEnum.Get,
                ["put"] = HttpMethodEnum.Put,
                ["post"] = HttpMethodEnum.Post
            };

        public HttpMethodEnum Methods;
        public Regex UrlRegex;

        public RequestFilterAttribute(HttpMethodEnum methods, string urlPattern)
        {
            Methods = methods;
            UrlRegex = new Regex(urlPattern);
        }

        public bool MatchRequest(HttpListenerRequest request)
        {
            return Methods.HasFlag(ConvertToEnum(request.HttpMethod)) && UrlRegex.IsMatch(request.RawUrl);
        }

        public Match GetMatchResult(HttpListenerRequest request)
        {
            return UrlRegex.Match(request.RawUrl);
        }

        private HttpMethodEnum ConvertToEnum(string httpMethod)
        {
            return httpMethodMap[httpMethod.ToLower()];
        }
    }
}