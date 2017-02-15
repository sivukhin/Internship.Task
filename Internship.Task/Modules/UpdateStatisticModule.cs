using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Internship.HandlerResult;
using Internship.Models;
using Internship.Storage;

namespace Internship.Modules
{
    public class UpdateStatisticModule : IServerModule
    {
        private static readonly Regex updateServerRegex = new Regex("^/servers/(?<serverId>.*?)/info");
        private static readonly Regex addMatchRegex = new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)");
        private readonly IStatisticStorage statisticStorage;

        public UpdateStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public IDisposable Subscribe(IObservable<HttpListenerContext> eventStream)
        {
            eventStream = eventStream.FilterMethod(HttpMethodEnum.Put);

            var updateServer = eventStream.FilterRequestString(updateServerRegex,
                (request, match) => UpdateServerInfo(request, match.Groups["serverId"].Value));

            var addMatch = eventStream.FilterRequestString(addMatchRegex,
                (request, match) => AddMatchStatistic(request,
                    match.Groups["serverId"].Value,
                    DateTime.Parse(match.Groups["endTime"].Value)));

            return updateServer.DisposeWith(addMatch);
        }

        public async Task<IHandlerResult> UpdateServerInfo(HttpListenerRequest request, string serverId)
        {
            var serverInfo = request.InputStream.ParseFromJson<ServerInfo>();
            await statisticStorage.UpdateServerInfo(serverId, serverInfo);
            return new BaseHandlerResult(HttpStatusCode.OK);
        }

        public async Task<IHandlerResult> AddMatchStatistic(HttpListenerRequest request, string serverId,
            DateTime endTime)
        {
            var matchInfo = request.InputStream.ParseFromJson<MatchInfo>();

            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new BaseHandlerResult(HttpStatusCode.BadRequest);
            await statisticStorage.UpdateMatchInfo(serverId, endTime, matchInfo);
            return new BaseHandlerResult(HttpStatusCode.OK);
        }
    }
}