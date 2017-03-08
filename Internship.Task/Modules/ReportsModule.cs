using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using NLog;

namespace StatisticServer.Modules
{
    public class ReportsModule : BaseModule
    {
        private int DefaultCountParameter => 5;
        private ILogger logger;
        protected override ILogger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(HttpMethodEnum.Get, new Regex(@"^/reports/recent-matches(?<count>/.*)?$"), 
                HandleRecentMatchesQuery), 
            new RequestFilter(HttpMethodEnum.Get, new Regex(@"^/reports/best-players(?<count>/.*)$"), 
                HandleBestPlayersQuery), 
            new RequestFilter(HttpMethodEnum.Get, new Regex(@"^/reporst/popular-servers(?<count>/.*)$"), 
                HandlePopularServersQuery), 
        };

        private int ParseIntegerGroupOrDefault(Group matchGroup, int defaultValue)
        {
            var result = defaultValue;
            if (matchGroup.Success)
                int.TryParse(matchGroup.Value, out result);
            return result;
        }

        private Task<IResponse> HandlePopularServersQuery(IRequest request, Match match)
        {
            var serversCount = ParseIntegerGroupOrDefault(match.Groups["count"], DefaultCountParameter);
            return GetPopularServers(serversCount);
        }

        private Task<IResponse> GetPopularServers(int serversCount)
        {
            throw new NotImplementedException();
        }

        private Task<IResponse> HandleBestPlayersQuery(IRequest request, Match match)
        {
            var playersCount = ParseIntegerGroupOrDefault(match.Groups["count"], DefaultCountParameter);
            return GetBestPlayers(playersCount);
        }

        private Task<IResponse> GetBestPlayers(int playersCount)
        {
            throw new NotImplementedException();
        }

        private Task<IResponse> HandleRecentMatchesQuery(IRequest request, Match match)
        {
            int matchCount = ParseIntegerGroupOrDefault(match.Groups["count"], DefaultCountParameter);
            return GetRecentMatches(matchCount);
        }

        private Task<IResponse> GetRecentMatches(int matchCount)
        {
            throw new NotImplementedException();
        }
    }
}
