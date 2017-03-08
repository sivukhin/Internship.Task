using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;

namespace HttpServerCore
{
    public class HttpServer : IHttpServer
    {
        private bool isDisposed;
        private readonly HttpListener listener;
        private readonly HttpServerOptions options;
        //TODO: I really need tasks?
        private IObservable<IObservable<Task<IRequest>>> requestStream;
        private IDisposable stopStreamToken;

        private Logger logger = LogManager.GetCurrentClassLogger();

        public HttpServer(HttpServerOptions options, IEnumerable<IServerModule> modules)
        {
            logger.Info("Initialize HttpServer");

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
            listener.Start();
            ConfigureListener();
            stopStreamToken = StartRequestStream();

            logger.Info("HttpServer started");
        }

        public void Stop()
        {
            if (!listener.IsListening)
                return;
            listener.Stop();
            StopRequestStream();

            logger.Info("HttpServer stopped");
        }

        public void RegisterModule(IServerModule module)
        {
            logger.Info("Register module {0}", module);

            var wasStarted = listener.IsListening;
            if (wasStarted)
                Stop();

            requestStream = requestStream.Select(
                innerStream => innerStream
                    .ObserveOn(Scheduler.Default)
                    //TODO: confusing construction for me...
                    .Select(async request => await module.ProcessRequest(await request)));

            if (wasStarted)
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

        private IObservable<IObservable<Task<IRequest>>> CreateRequestStream()
        {
            return Observable
                .FromAsync(listener.GetContextAsync)
                .Repeat()
                .Retry()
                .Select(context => (IRequest)new HttpRequest(context))
                .Do(request => logger.Trace("New request {0}", request))
                //TODO: this can be configured in HttpServerOptions
                .Window(1)
                .Select(innerStream => innerStream.Select(Task.FromResult));
        }

        private void ConfigureListener()
        {
            listener.Prefixes.Clear();
            listener.Prefixes.Add(options.Prefix);
        }

        private IDisposable StartRequestStream()
        {
            return requestStream
                .Subscribe(
                    innerStream => innerStream
                        .ObserveOn(Scheduler.Default)
                        .Subscribe(async request =>
                        {
                            var result = await request;
                            logger.Trace("Request {0} processed", result);
                            result.SendAttachedResponse();
                        }));
        }

        private void StopRequestStream()
        {
            stopStreamToken?.Dispose();
            stopStreamToken = null;
        }
    }
}