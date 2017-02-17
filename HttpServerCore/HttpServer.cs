using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace HttpServerCore
{
    public class HttpServer : IHttpServer
    {
        private bool isDisposed;
        private readonly HttpListener listener;
        private readonly HttpServerOptions options;
        private IObservable<IObservable<IRequest>> requestStream;
        private IDisposable stopStreamToken;

        public HttpServer(HttpServerOptions options, IEnumerable<IServerModule> modules)
        {
            this.options = options;
            listener = new HttpListener();
            requestStream = CreateRequestStream();
            foreach (var module in modules)
                RegisterModule(module);
        }

        public void Start()
        {
            if (listener.IsListening)
                return;
            ConfigureListener();
            listener.Start();
            StartRequestStream();
            stopStreamToken = requestStream.Publish().Connect();
        }

        public void Stop()
        {
            if (!listener.IsListening)
                return;
            listener.Stop();
            StopRequestStream();
        }

        public void RegisterModule(IServerModule module)
        {
            bool started = listener.IsListening;
            if (started)
                Stop();

            requestStream = requestStream.Select(
                innerStream => innerStream
                    .ObserveOn(Scheduler.Default)
                    .Select(module.ProcessRequest));

            if (started)
                Start();
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            Stop();
            listener.Close();
        }

        private IObservable<IObservable<IRequest>> CreateRequestStream()
        {
            return Observable
                .FromAsync(listener.GetContextAsync)
                .Repeat()
                .Retry()
                .Select(context => new HttpRequest(context))
                .Cast<IRequest>()
                //TODO: this can be configured in HttpServerOptions
                .Window(1);
        }

        private void ConfigureListener()
        {
            listener.Prefixes.Clear();
            listener.Prefixes.Add(options.Prefix);
        }

        private void StartRequestStream()
        {
            stopStreamToken = requestStream
                .Select(
                    innerStream => innerStream
                        .ObserveOn(Scheduler.Default)
                        .Subscribe(request => request.SendAttachedResponse()))
                .Publish()
                .Connect();
        }

        private void StopRequestStream()
        {
            stopStreamToken?.Dispose();
            stopStreamToken = null;
        }
    }
}