using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Internship.Modules
{
    public abstract class BaseServerModule : IServerModule
    {
        //TODO: change this scary list for more attractive collection
        private readonly List<Tuple<RequestFilterAttribute, Action<HttpListenerContext, Match>>> handlers;
        protected BaseServerModule()
        {
            handlers = new List<Tuple<RequestFilterAttribute, Action<HttpListenerContext, Match>>>();
            InitializeHandlers();
        }

        private void AddHanlder(RequestFilterAttribute filter, Action<HttpListenerContext, Match> handler)
        {
            handlers.Add(Tuple.Create(filter, handler));
        }

        private void InitializeHandlers()
        {
            foreach (var handler in GetType().GetMethods())
            {
                var requestFilter = handler.GetCustomAttribute<RequestFilterAttribute>();
                if (requestFilter == null)
                    continue;
                AddHanlder(requestFilter, async (context, matchResult) =>
                {
                    var parameters = new List<object> {context};
                    foreach (var parameterInfo in handler.GetParameters().Skip(1))
                        parameters.Add(matchResult.Groups[parameterInfo.Name.ToLower()].Value);
                    var handlerResult = (Task<IHandlerResult>)handler.Invoke(this, parameters.ToArray());
                    await handlerResult;
                    handlerResult.Result?.WriteResult(context.Response);
                });
            }
        }

        public bool TryProcessRequest(HttpListenerContext context)
        {
            foreach (var handler in handlers)
            {
                if (handler.Item1.MatchRequest(context.Request))
                {
                    handler.Item2(context, handler.Item1.GetMatchResult(context.Request));
                    return true;
                }
            }
            return false;
        }

        protected T ParseRequestJson<T>(Stream requestInputStream)
        {
            var serializer = new JsonSerializer();
            using (var inputReader = new StreamReader(requestInputStream))
            using (var jsonReader = new JsonTextReader(inputReader))
            {
                return serializer.Deserialize<T>(jsonReader);
            }
        }
    }
}