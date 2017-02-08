using System.Net;

namespace Internship
{
    public interface IServerModule
    {
        bool TryProcessRequest(HttpListenerContext context);
    }
}