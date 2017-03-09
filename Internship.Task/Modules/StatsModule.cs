using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using NLog;
using StatisticServer.Storage;

namespace StatisticServer.Modules
{
    public class StatsModule : BaseModule
    {
        private ILogger logger;
        protected override ILogger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/servers/(?<serverId>.*?)/stats$", RegexOptions.Compiled), 
                (request, match) => GetFullServerStatistic(match.Groups["serverId"].Value)),
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/players/(?<name>.*?)/stats$", RegexOptions.Compiled), 
                (request, match) => GetFullPlayerStatstic(match.Groups["name"].Value)),
        };

        private readonly IStatisticStorage statisticStorage;
        public StatsModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        private async Task<IResponse> GetFullServerStatistic(string serverId)
        {
            var serverStatistics = await statisticStorage.GetServerStatistics(serverId);
            return new JsonHttpResponse(HttpStatusCode.OK, serverStatistics);
        }

        private Task<IResponse> GetFullPlayerStatstic(string playerName)
        {
            throw new NotImplementedException();
        }
    }
}
