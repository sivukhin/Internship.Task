using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using Kontur.GameStats.Server.Storage;
using NLog;

namespace Kontur.GameStats.Server.Modules
{
    public class StatisticModule : BaseModule
    {
        private Logger logger;
        protected override Logger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/servers/(?<serverId>[^/]*)/stats$", RegexOptions.Compiled), 
                (request, match) => GetFullServerStatistic(match.Groups["serverId"].Value)),
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/players/(?<name>[^/]*)/stats$", RegexOptions.Compiled), 
                (request, match) => GetFullPlayerStatstic(match.Groups["name"].Value)),
        };

        private readonly IServerStatisticStorage serverStatistics;
        private readonly IPlayerStatisticStorage playerStatistics;

        public StatisticModule(IServerStatisticStorage serverStatistics, IPlayerStatisticStorage playerStatistics)
        {
            this.serverStatistics = serverStatistics;
            this.playerStatistics = playerStatistics;
        }

        private Task<IResponse> GetFullServerStatistic(string serverId)
        {
            var statistics = serverStatistics.GetStatistics(serverId);
            if (statistics.TotalMatchesPlayed == 0)
                return Task.FromResult<IResponse>(new HttpResponse(HttpStatusCode.NotFound));
            return Task.FromResult<IResponse>(new JsonHttpResponse(HttpStatusCode.OK, statistics));
        }

        private Task<IResponse> GetFullPlayerStatstic(string playerName)
        {
            var statistics = playerStatistics.GetStatistics(playerName);
            if (statistics.TotalMatchesPlayed == 0)
                return Task.FromResult<IResponse>(new HttpResponse(HttpStatusCode.NotFound));
            
            return Task.FromResult<IResponse>(new JsonHttpResponse(HttpStatusCode.OK, new
            {
                statistics.TotalMatchesPlayed,
                statistics.AverageMatchesPerDay,
                statistics.AverageScoreboardPercent,
                statistics.FavoriteGameMode,
                statistics.FavoriteServer,
                lastMatchPlayed = statistics.LastMatchPlayed.ToUtcFormat(),
                statistics.MaximumMatchesPerDay,
                statistics.TotalMatchesWon,
                statistics.UniqueServers,
                statistics.KillToDeathRatio,
            }));
        }
    }
}
