using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerCore
{
    public class HttpRequest : IRequest
    {
        public Uri Url => context.Request.Url;
        public HttpMethodEnum HttpMethod => context.Request.HttpMethod.ToEnum();
        public string Content { get; private set; }
        public IResponse Response { get; set; }

        //TODO: for informative logs
        public string[] AcceptTypes => context.Request.AcceptTypes;
        public Encoding ContentEncoding => context.Request.ContentEncoding;
        public string ContentType => context.Request.ContentType;
        public long ContentLength => context.Request.ContentLength64;
        public CookieCollection Cookies => context.Request.Cookies;
        public NameValueCollection Headers => context.Request.Headers;
        public NameValueCollection QueryString => context.Request.QueryString;
        public Version ProtocolVersion => context.Request.ProtocolVersion;

        private readonly HttpListenerContext context;

        //TODO: set defaultResponse via some options?
        private static readonly IResponse defaultResponse = new HttpResponse(HttpStatusCode.NotFound);

        public HttpRequest(HttpListenerContext context)
        {
            this.context = context;
            InitializeContent();
        }

        private void InitializeContent()
        {
            using (var streamReader = new StreamReader(context.Request.InputStream))
                Content = streamReader.ReadToEnd();
        }

        //TODO: behaviour when send response many times?
        public void SendAttachedResponse()
        {
            if (Response == null)
                Response = defaultResponse;
            Response.WriteToListenerResponse(context.Response);
            context.Response.Close();
        }

        public override string ToString()
        {
            return $"HttpRequest({Url}, {HttpMethod}, \"{Content}\")";
        }
    }
}
