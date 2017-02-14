using System;
using System.Net;
using System.Reactive;

namespace Internship.Modules
{
    public interface IServerModule
    {
        IDisposable Subscribe(IObservable<HttpListenerContext> eventStream);
    }
}