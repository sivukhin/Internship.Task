using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Internship
{
    public class StatisticsServer : IDisposable
    {
        private ServerOptions options;
        private readonly HttpListener listener = new HttpListener();
        private Thread listeningThread;
        private bool isDisposed;
        private readonly List<IServerModule> modules;

        public StatisticsServer(ServerOptions options, IEnumerable<IServerModule> modules)
        {
            this.options = options;
            this.modules = modules.ToList();
        }

        public void StartListen()
        {
            if (listener.IsListening) return;

            listener.Prefixes.Clear();
            listener.Prefixes.Add(options.Prefix);
            listener.Start();

            listeningThread = new Thread(Listen) {IsBackground = true, Priority = ThreadPriority.Highest};
            listeningThread.Start();
        }

        private void Listen()
        {
            while (true)
            {
                var context = listener.GetContext();
                DistributeRequest(context);
            }
        }

        private void DistributeRequest(HttpListenerContext context)
        {
            foreach (var module in modules)
                if (module.TryProcessRequest(context))
                    break;
        }
        
        public void StopListen()
        {
            if (listener.IsListening)
            {
                listeningThread.Abort();
                listener.Stop();
            }
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            isDisposed = true;
            StopListen();
            listener.Close();
        }
    }
}