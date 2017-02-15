using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Internship.Modules
{
    public static class ObservableHttpListenerShortcuts
    {
        private static readonly Dictionary<string, HttpMethodEnum> httpMethodMap =
            new Dictionary<string, HttpMethodEnum>
            {
                ["get"] = HttpMethodEnum.Get,
                ["put"] = HttpMethodEnum.Put,
                ["post"] = HttpMethodEnum.Post
            };

        public static IObservable<HttpListenerContext> FilterMethod(this IObservable<HttpListenerContext> eventStream,
            HttpMethodEnum methods)
        {
            return eventStream.Where(context => methods.HasFlag(ToEnum(context.Request.HttpMethod)));
        }

        public static IObservable<HttpListenerContext> FilterRequestString(this IObservable<HttpListenerContext> eventStream,
            Regex pattern)
        {
            return eventStream.Where(context => pattern.IsMatch(context.Request.RawUrl));
        }

        public static IDisposable FilterRequestString(this IObservable<HttpListenerContext> eventStream,
           Regex pattern, Func<HttpListenerRequest, Match, IHandlerResult> subscription)
        {
            return eventStream
                .Select(context => Tuple.Create(context, pattern.Match(context.Request.RawUrl)))
                .Where(tuple => tuple.Item2.Success)
                .Subscribe(tuple =>
                {
                    var response = subscription(tuple.Item1.Request, tuple.Item2);
                    response.WriteResult(tuple.Item1.Response);
                });
        }

        public static IDisposable FilterRequestString(this IObservable<HttpListenerContext> eventStream,
           Regex pattern, Func<HttpListenerRequest, Match, Task<IHandlerResult>> subscription)
        {
            return eventStream
                .Select(context => Tuple.Create(context, pattern.Match(context.Request.RawUrl)))
                .Where(tuple => tuple.Item2.Success)
                .Subscribe(async tuple =>
                {
                    var response = await subscription(tuple.Item1.Request, tuple.Item2);
                    response.WriteResult(tuple.Item1.Response);
                });
        }

        public static IDisposable Subscribe(this IObservable<HttpListenerContext> eventStream,
            Func<HttpListenerRequest, IHandlerResult> handler)
        {
            return eventStream.Subscribe(context =>
            {
                var response = handler(context.Request);
                response.WriteResult(context.Response);
            });
        }

        public static IDisposable SubscribeAsync(this IObservable<HttpListenerContext> eventStream,
           Func<HttpListenerRequest, Task<IHandlerResult>> handler)
        {
            return eventStream.Subscribe(async context =>
            {
                var response = await handler(context.Request);
                response.WriteResult(context.Response);
            });
        }


        public static IDisposable DisposeWith(this IDisposable first, params IDisposable[] others)
        {
            return Disposable.Create(() =>
            {
                first.Dispose();
                foreach (var other in others)
                    other.Dispose();
            });
        }

        public static HttpMethodEnum ToEnum(this string httpMethod)
        {
            return httpMethodMap[httpMethod.ToLower()];
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
    }
}