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
        private Logger logger;
        protected override Logger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new []
        {
            new RequestFilter(
                HttpMethodEnum.Put, 
                new Regex("^/servers/(?<serverId>[^/]*)/info$", RegexOptions.Compiled), 
                (request, match) => UpdateServerInfo(request, match.Groups["serverId"].Value)),
            
            new RequestFilter(
                HttpMethodEnum.Put,
                new Regex("^/servers/(?<serverId>[^/]*)/matches/(?<endTime>.*)$", RegexOptions.Compiled),
                (request, match) => AddMatchStatistic(request, match.Groups["serverId"].Value, DateTime.Parse(match.Groups["endTime"].Value)))
        };

        private readonly IDataStatisticStorage dataStatisticStorage;

        public UpdateStatisticModule(IDataStatisticStorage storage)
        {
            dataStatisticStorage = storage;
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
            await dataStatisticStorage.UpdateServer(new ServerInfo.ServerInfoId {Id = serverId}, serverInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }

        public async Task<IResponse> AddMatchStatistic(IRequest request, string serverId,
            DateTime endTime)
        {
            MatchInfo matchInfo;
            try
            {
                matchInfo = request.Content.ParseFromJson<MatchInfo>().InitPlayers(endTime);
            }
            catch (JsonSerializationException e)
            {
                throw new InvalidQueryException($"Invalid json format for update module: {request.Content}", e);
            }

            var serverInfo = await dataStatisticStorage.GetServer(new ServerInfo.ServerInfoId {Id = serverId});
            if (serverInfo == null)
                return new HttpResponse(HttpStatusCode.BadRequest);
            await dataStatisticStorage.UpdateMatch(new MatchInfo.MatchInfoId {ServerId = serverId, EndTime = endTime}, matchInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }
    }
}