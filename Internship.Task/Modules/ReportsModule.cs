using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataCore;
using HttpServerCore;
using NLog;
using StatisticServer.Storage;

namespace StatisticServer.Modules
{
    public class ReportsModule : BaseModule
    {
        private readonly IAggregateReportStorage reportStorage;
        private readonly int DefaultCountParameter = 5;
        private int MinCountParameter = 0;
        private int MaxCountParameter = 50;
        private ILogger logger;
        protected override ILogger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        public ReportsModule(IAggregateReportStorage reportStorage)
        {
            this.reportStorage = reportStorage;
        }

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(HttpMethodEnum.Get, 
                new Regex(@"^/reports/recent-matches(/?<count>.*)?$", RegexOptions.Compiled), 
                HandleRecentMatchesQuery), 
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex(@"^/reports/best-players(/?<count>.*)?$", RegexOptions.Compiled), 
                HandleBestPlayersQuery), 
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex(@"^/reports/popular-servers(/?<count>.*)?$", RegexOptions.Compiled), 
                HandlePopularServersQuery), 
        };

        private int ParseIntegerGroupOrDefault(Group matchGroup, int defaultValue)
        {
            var result = defaultValue;
            if (matchGroup.Success)
                int.TryParse(matchGroup.Value, out result);
            return result;
        }

        private int FitInBound(int value, int lowerBound, int upperBound)
        {
            if (value < lowerBound)
                return lowerBound;
            if (value > upperBound)
                return upperBound;
            return value;
        }

        private int FilterCountParameter(Group group)
        {
            return FitInBound(
                ParseIntegerGroupOrDefault(group, DefaultCountParameter), 
                MinCountParameter,
                MaxCountParameter);
        }

        private Task<IResponse> HandlePopularServersQuery(IRequest request, Match match)
        {
            var serversCount = FilterCountParameter(match.Groups["count"]);
            return GetPopularServers(serversCount);
        }

        private Task<IResponse> GetPopularServers(int serversCount)
        {
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, reportStorage.PopularServers(serversCount));
            return Task.FromResult(response);
        }

        private Task<IResponse> HandleBestPlayersQuery(IRequest request, Match match)
        {
            var playersCount = FilterCountParameter(match.Groups["count"]);
            return GetBestPlayers(playersCount);
        }

        private Task<IResponse> GetBestPlayers(int playersCount)
        {
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, reportStorage.BestPlayers(playersCount));
            return Task.FromResult(response);
        }

        private Task<IResponse> HandleRecentMatchesQuery(IRequest request, Match match)
        {
            var matchCount = FilterCountParameter(match.Groups["count"]);
            return GetRecentMatches(matchCount);
        }

        private Task<IResponse> GetRecentMatches(int matchCount)
        {
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, reportStorage.RecentMatches(matchCount));
            return Task.FromResult(response);
        }
    }
}
