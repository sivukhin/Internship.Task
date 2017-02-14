using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Internship.Modules;

namespace Internship
{
    public class StatisticsServer : IDisposable, IObservable<HttpListenerContext>
    {
        private readonly HttpListener listener = new HttpListener();
        private readonly IConnectableObservable<HttpListenerContext> listenerEventStream;
        private readonly List<IDisposable> modulesDisposeTokens;
        private readonly ServerOptions options;
        private IDisposable eventStreamDisposeToken;
        private bool isDisposed;

        public StatisticsServer(ServerOptions options, IEnumerable<IServerModule> modules)
        {
            this.options = options;
            listenerEventStream = CreateListenerEventStream();
            modulesDisposeTokens = new List<IDisposable>();

            foreach (var module in modules)
                RegisterModule(module);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            StopListen();
            listener.Close();

            foreach (var disposeToken in modulesDisposeTokens)
                disposeToken.Dispose();
        }

        public IDisposable Subscribe(IObserver<HttpListenerContext> observer)
        {
            return listenerEventStream.Subscribe(observer);
        }

        public IDisposable RegisterModule(IServerModule module)
        {
            var disposeToken = module.Subscribe(this);
            modulesDisposeTokens.Add(disposeToken);
            return disposeToken;
        }

        public void StartListen()
        {
            if (listener.IsListening) return;

            listener.Prefixes.Clear();
            listener.Prefixes.Add(options.Prefix);
            listener.Start();

            eventStreamDisposeToken = listenerEventStream.Connect();
        }

        public void StopListen()
        {
            if (listener.IsListening)
            {
                eventStreamDisposeToken.Dispose();
                listener.Stop();
            }
        }

        private IConnectableObservable<HttpListenerContext> CreateListenerEventStream()
        {
            return Observable.FromAsync(listener.GetContextAsync)
                .Repeat()
                .Retry()
                .Publish();
        }
    }
}