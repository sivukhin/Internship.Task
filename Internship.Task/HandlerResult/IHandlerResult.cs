using System.Net;

namespace Internship
{
    public interface IHandlerResult
    {
        void WriteResult(HttpListenerResponse response);
    }
}