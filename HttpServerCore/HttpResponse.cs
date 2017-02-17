using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpServerCore
{
    public class HttpResponse : IResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }

        public HttpResponse(HttpStatusCode statusCode, string content = "")
        {
            StatusCode = statusCode;
            Content = content;
        }
    }

    public class JsonHttpResponse : IResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }

        public JsonHttpResponse(HttpStatusCode statusCode, object data)
        {
            StatusCode = statusCode;
            Content = HttpServerExtensions.Serialize(data);
        }
    }
}
