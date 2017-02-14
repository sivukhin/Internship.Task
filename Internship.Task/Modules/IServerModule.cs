using System;
using System.Net;
using System.Reactive;

namespace Internship.Modules
{
    public interface IServerModule
    {
        IObservable<HttpListenerContext> FilterSubscription(IObservable<HttpListenerContext> eventStream);
        IHandlerResult ProcessRequest(HttpListenerRequest request);
    }

    public static class ServerModuleExtensions
    {
        public static IObserver<HttpListenerContext> ToObserver(this IServerModule module)
        {
            return Observer.Create((HttpListenerContext context) =>
            {
                var result = module.ProcessRequest(context.Request);
                result.WriteResult(context.Response);
            });
        }
    }
}