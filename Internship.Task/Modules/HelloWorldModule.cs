using System;
using System.Net;
using System.Text.RegularExpressions;
using Internship.HandlerResult;

namespace Internship.Modules
{
    public class HelloWorldModule : IServerModule
    {
        public IDisposable Subscribe(IObservable<HttpListenerContext> eventStream)
        {
            eventStream = eventStream.FilterMethod(HttpMethodEnum.Get);
            var hello = eventStream
                .FilterRequestString(new Regex("^/hello$"))
                .Subscribe(HandleHello);
            var goodbye = eventStream
                .FilterRequestString(new Regex("^/goodbye$"))
                .Subscribe(HandleGoodBye);
            return hello.DisposeWith(goodbye);
        }

        public IHandlerResult HandleHello(HttpListenerRequest request)
        {
            return new BaseHandlerResult(HttpStatusCode.OK, "Hello, world!");
        }

        public IHandlerResult HandleGoodBye(HttpListenerRequest request)
        {
            return new BaseHandlerResult(HttpStatusCode.OK, "Good bye, world!");
        }
    }
}