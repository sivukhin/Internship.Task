using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                jsonWriter.Flush();
                memoryStream.Position = 0;
                return Encoding.UTF8.GetString(memoryStream.ToArray());
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

        public static IRequest AttachResponse(this IRequest request, IResponse response)
        {
            request.Response = response;
            return request;
        }

        public static T ParseFromJson<T>(this string data)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                return stream.ParseFromJson<T>();
        }
        
        public static T ParseFromJson<T>(this Stream inputStream)
        {
            var serializer = new JsonSerializer();
            using (var inputReader = new StreamReader(inputStream))
            using (var jsonReader = new JsonTextReader(inputReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public static Match MatchLocalPath(this IRequest request, Regex regexPattern)
        {
            return regexPattern.Match(request.Url.LocalPath);
        }
    }
}
