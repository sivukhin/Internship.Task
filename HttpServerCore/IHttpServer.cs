using System;

namespace HttpServerCore
{
    public interface IHttpServer : IDisposable
    {
        void Start();
        void Stop();
        void RegisterModule(IServerModule module);
    }
}