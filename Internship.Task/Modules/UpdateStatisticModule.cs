using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataCore;
using HttpServerCore;
using Newtonsoft.Json;
using NLog;
using StatisticServer.Storage;

namespace StatisticServer.Modules
{
    public class UpdateStatisticModule : BaseModule
    {
        private ILogger logger;
        protected override ILogger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new []
        {
            new RequestFilter(
                HttpMethodEnum.Put, 
                new Regex("^/servers/(?<serverId>.*?)/info$", RegexOptions.Compiled), 
                (request, match) => UpdateServerInfo(request, match.Groups["serverId"].Value)),
            
            new RequestFilter(
                HttpMethodEnum.Put,
                new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)$", RegexOptions.Compiled),
                (request, match) => AddMatchStatistic(request, match.Groups["serverId"].Value, DateTime.Parse(match.Groups["endTime"].Value)))
        };

        private readonly IStatisticStorage statisticStorage;

        public UpdateStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public async Task<IResponse> UpdateServerInfo(IRequest request, string serverId)
        {
            ServerInfo serverInfo;
            try
            {
                serverInfo = request.Content.ParseFromJson<ServerInfo>();
            }
            catch (JsonSerializationException e)
            {
                throw new InvalidQueryException($"Invalid json format for update module: {request.Content}", e);
            }
            await statisticStorage.UpdateServerInfo(serverId, serverInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }

        public async Task<IResponse> AddMatchStatistic(IRequest request, string serverId,
            DateTime endTime)
        {
            MatchInfo matchInfo;
            try
            {
                matchInfo = request.Content.ParseFromJson<MatchInfo>();
            }
            catch (JsonSerializationException e)
            {
                throw new InvalidQueryException($"Invalid json format for update module: {request.Content}", e);
            }

            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new HttpResponse(HttpStatusCode.BadRequest);
            await statisticStorage.UpdateMatchInfo(serverId, endTime, matchInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }
    }
}