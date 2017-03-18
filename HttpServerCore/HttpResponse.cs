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

        public override string ToString()
        {
            return $"HttpResponse({StatusCode}, \"{Content}\")";
        }

        protected bool Equals(HttpResponse other)
        {
            return StatusCode == other.StatusCode && string.Equals(Content, other.Content);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((HttpResponse)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)StatusCode * 397) ^ (Content?.GetHashCode() ?? 0);
            }
        }

    }

    public class JsonHttpResponse: HttpResponse
    {
        public JsonHttpResponse(HttpStatusCode statusCode, object data) :
            base(statusCode, JsonConvert.SerializeObject(data))
        {
            
        }
    }
}
