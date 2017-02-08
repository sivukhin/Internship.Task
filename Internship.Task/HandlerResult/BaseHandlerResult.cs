using System.IO;
using System.Net;

namespace Internship
{
    public class BaseHandlerResult : IHandlerResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Payload { get; set; }
        public BaseHandlerResult(HttpStatusCode statusCode, string payload = "")
        {
            StatusCode = statusCode;
            Payload = payload;
        }
        
        public void WriteResult(HttpListenerResponse response)
        {
            response.StatusCode = (int)StatusCode;
            using (var outputStream = new StreamWriter(response.OutputStream))
                outputStream.Write(Payload);
        }
    }
}