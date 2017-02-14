using System;
using System.IO;
using System.Net;
using System.Reactive.Linq;

namespace Internship.Modules
{
    internal class HelloWorldModule : IServerModule
    {
        public IHandlerResult ProcessRequest(HttpListenerRequest request)
        {
            return new BaseHandlerResult(HttpStatusCode.OK, $"{request.RawUrl.Substring(1)}, world!");
        }

        public IObservable<HttpListenerContext> FilterSubscription(IObservable<HttpListenerContext> eventStream)
        {
            return eventStream.Where(context =>
                context.Request.RawUrl.StartsWith("/hello") ||
                context.Request.RawUrl.StartsWith("/bye"));
        }
    }
}