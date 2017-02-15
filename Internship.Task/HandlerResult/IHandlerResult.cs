using System.Net;

namespace Internship.HandlerResult
{
    public interface IHandlerResult
    {
        void WriteResult(HttpListenerResponse response);
    }
}