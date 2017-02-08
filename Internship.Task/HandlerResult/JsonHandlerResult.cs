using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Internship
{
    public class JsonHandlerResult<T> : IHandlerResult 
        where T:class
    {
        public HttpStatusCode StatusCode { get; set; }
        public T Data { get; set; }

        public JsonHandlerResult(HttpStatusCode statusCode, T data = null)
        {
            StatusCode = statusCode;
            Data = data;
        }

        public void WriteResult(HttpListenerResponse response)
        {
            response.StatusCode = (int)StatusCode;
            if (Data == null)
                return;
            var serializer = new JsonSerializer();
            using (var outputStream = new StreamWriter(response.OutputStream))
            using (var jsonStream = new JsonTextWriter(outputStream))
            {
                serializer.Serialize(jsonStream, Data);
            }
        }
    }
}