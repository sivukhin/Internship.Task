using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace HttpServerCore
{
    public class HttpServer : IHttpServer
    {
        private bool isDisposed;
        private readonly HttpListener listener;
        private readonly HttpServerOptions options;
        private readonly IObservable<HttpListenerContext> requestStream;
        private IDisposable stopStreamToken;
        private readonly List<IServerModule> registredModules;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpServer(HttpServerOptions options, IEnumerable<IServerModule> modules)
        {
            logger.Info("Initialize HttpServer");

            this.options = options;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.DefaultConnectionLimit = 10000;
            listener = new HttpListener();
            ThreadPool.SetMaxThreads(10000, 10000);

            requestStream = CreateRequestStream();
            registredModules = modules.ToList();
        }

        public void Start()
        {
            if (listener.IsListening)
                return;
            ConfigureListener();
            listener.Start();
            stopStreamToken = StartRequestStream();

            logger.Info("HttpServer started");
        }

        public void Stop()
        {
            if (!listener.IsListening)
                return;
            StopRequestStream();
            listener.Stop();

            logger.Info("HttpServer stopped");
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            Stop();
            listener.Close();
        }

        private IObservable<HttpListenerContext> CreateRequestStream()
        {
            return Observable
                .FromAsync(listener.GetContextAsync)
                .Repeat()
                .Retry();
        }

        private void ConfigureListener()
        {
            listener.Prefixes.Clear();
            listener.Prefixes.Add(options.Prefix);
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        }

        private IDisposable StartRequestStream()
        {
            return requestStream
                .Subscribe(context =>
                {
                    Task.Run(() => ProcessContext(context));
                });
        }

        private async void ProcessContext(HttpListenerContext context)
        {
            IRequest request = new HttpRequest(context);
            foreach (var module in registredModules)
                request = await module.ProcessRequest(request);
            try
            {
                await request.SendAttachedResponseAsync();
            }
            catch (Exception e)
            {
                logger.Warn(e, "Unhandled exception");
            }
        }

        private void StopRequestStream()
        {
            stopStreamToken?.Dispose();
            stopStreamToken = null;
        }
    }
}