using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Internship.Storage;

namespace Internship.Modules
{
    public class GetStatisticModule : IServerModule
    {
        private static readonly Regex getServerInfoRegex = new Regex("^/servers/(?<serverId>.*?)/info");
        private static readonly Regex getAllServersInfoRegex = new Regex("^/servers/info");
        private static readonly Regex getMatchInfoRegex = new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)");

        private readonly IStatisticStorage statisticStorage;
        public GetStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public async Task<IHandlerResult> GetServerInfo(HttpListenerRequest request, string serverId)
        {
            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new BaseHandlerResult(HttpStatusCode.NotFound);
            return new JsonHandlerResult(HttpStatusCode.OK, serverInfo);
        }

        public async Task<IHandlerResult> GetAllServersInfo(HttpListenerRequest request)
        {
            var allServersInfo = await statisticStorage.GetAllServersInfo();
            return new JsonHandlerResult(HttpStatusCode.OK, allServersInfo);
        }

        public async Task<IHandlerResult> GetMatchInfo(HttpListenerRequest request, string serverId, DateTime endTime)
        {
            var matchInfo = await statisticStorage.GetMatchInfo(serverId, endTime);
            if (matchInfo == null)
                return new BaseHandlerResult(HttpStatusCode.NotFound);
            return new JsonHandlerResult(HttpStatusCode.OK, matchInfo);
        }

        public IDisposable Subscribe(IObservable<HttpListenerContext> eventStream)
        {
            eventStream = eventStream.FilterMethod(HttpMethodEnum.Get);

            var getServerInfo = eventStream.FilterRequestString(getServerInfoRegex,
                (request, match) => GetServerInfo(request, match.Groups["serverId"].Value));

            var getAllServersInfo = eventStream
                .FilterRequestString(getAllServersInfoRegex)
                .SubscribeAsync(GetAllServersInfo);

            var getMatchInfo = eventStream.FilterRequestString(getMatchInfoRegex, 
                (request, match) => GetMatchInfo(request, match.Groups["serverId"].Value, DateTime.Parse(match.Groups["endTime"].Value)));

            return getServerInfo.DisposeWith(getAllServersInfo, getMatchInfo);
        }
    }
}
