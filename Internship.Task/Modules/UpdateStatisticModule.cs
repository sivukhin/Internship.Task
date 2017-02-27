using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataCore;
using HttpServerCore;
using StatisticServer.Storage;

namespace StatisticServer.Modules
{
    public class UpdateStatisticModule : BaseModule
    {
        protected override IEnumerable<RequestFilter> Filters => new []
        {
            new RequestFilter(HttpMethodEnum.Put, new Regex("^/servers/(?<serverId>.*?)/info$"), 
                (request, match) => UpdateServerInfo(request, match.Groups["serverId"].Value)),
            
            new RequestFilter(HttpMethodEnum.Put, new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)$"), 
                (request, match) => AddMatchStatistic(request, match.Groups["serverId"].Value, DateTime.Parse(match.Groups["endTime"].Value)))
        };

        private readonly IStatisticStorage statisticStorage;

        public UpdateStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public async Task<IResponse> UpdateServerInfo(IRequest request, string serverId)
        {
            var serverInfo = request.Content.ParseFromJson<ServerInfo>();
            await statisticStorage.UpdateServerInfo(serverId, serverInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }

        public async Task<IResponse> AddMatchStatistic(IRequest request, string serverId,
            DateTime endTime)
        {
            var matchInfo = request.Content.ParseFromJson<MatchInfo>();

            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new HttpResponse(HttpStatusCode.BadRequest);
            await statisticStorage.UpdateMatchInfo(serverId, endTime, matchInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }
    }
}