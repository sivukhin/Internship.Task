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
