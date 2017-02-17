using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpServerCore
{
    public static class HttpServerExtensions
    {
        private static readonly Dictionary<string, HttpMethodEnum> httpMethodMap =
            new Dictionary<string, HttpMethodEnum>
            {
                ["get"] = HttpMethodEnum.Get,
                ["put"] = HttpMethodEnum.Put,
                ["post"] = HttpMethodEnum.Post
            };

        public static bool IsResponded(this IRequest request)
        {
            return request.Response != null;
        }

        public static string Serialize(object data)
        {
            var serializer = new JsonSerializer();
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                serializer.Serialize(jsonWriter, data);
                using (var streamReader = new StreamReader(memoryStream))
                    return streamReader.ReadToEnd();
            }
        }

        public static HttpMethodEnum ToEnum(this string httpMethod)
        {
            return httpMethodMap[httpMethod.ToLower()];
        }

        public static void WriteToListenerResponse(this IResponse response, HttpListenerResponse listenerResponse)
        {
            listenerResponse.StatusCode = (int)response.StatusCode;
            using (var responseStream = new StreamWriter(listenerResponse.OutputStream))
                responseStream.Write(response.Content);
        }
    }
}
