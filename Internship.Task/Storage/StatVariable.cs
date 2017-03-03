using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using StatCore;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public class ServerStatistic
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public double AveragePopulation { get; set; }
        public IEnumerable<string> Top5GameModes { get; set; }
        public IEnumerable<string> Top5Maps { get; set; }
    }

    public interface IServerStatisticProvider
    {
        ServerStatistic this[string serverId] { get; }
        void Add(MatchInfo match);
        void Delete(MatchInfo match);
    }

    public class ServerStatisticProvider : IServerStatisticProvider
    {
        private readonly GroupStat<MatchInfo, int, string> totalMatchesPlayed =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Count();

        private readonly GroupStat<MatchInfo, int, string> maximumMatchesPerDay =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .MaxByGroup(match => match.EndTime.Date, () => StatFor<MatchInfo>.Count());

        private readonly GroupStat<MatchInfo, double, string> averageMatchesPerDay =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .AverageByGroup(match => match.EndTime.Date, () => StatFor<MatchInfo>.Count().Select(i => (double)i));

        private readonly GroupStat<MatchInfo, int, string> maximumPopulation =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Max(match => match.Scoreboard.Count);

        private readonly GroupStat<MatchInfo, double, string> averagePopulation =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Average(match => match.Scoreboard.Count);

        private readonly GroupStat<MatchInfo, IEnumerable<string>, string> top5GameModes =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .Popular(5, match => match.GameMode.ModeName);

        private readonly GroupStat<MatchInfo, IEnumerable<string>, string> top5Maps =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .Popular(5, match => match.Map);


        public void Add(MatchInfo match)
        {
            totalMatchesPlayed.Add(match);
            maximumMatchesPerDay.Add(match);
            averageMatchesPerDay.Add(match);
            maximumPopulation.Add(match);
            averagePopulation.Add(match);
            top5GameModes.Add(match);
            top5Maps.Add(match);
        }

        public void Delete(MatchInfo match)
        {
            totalMatchesPlayed.Delete(match);
            maximumMatchesPerDay.Delete(match);
            averageMatchesPerDay.Delete(match);
            maximumPopulation.Delete(match);
            averagePopulation.Delete(match);
            top5GameModes.Delete(match);
            top5Maps.Delete(match);
        }

        public ServerStatistic this[string serverId] => new ServerStatistic
        {
            TotalMatchesPlayed = totalMatchesPlayed[serverId],
            MaximumMatchesPerDay = maximumMatchesPerDay[serverId],
            AverageMatchesPerDay = averageMatchesPerDay[serverId],
            MaximumPopulation = maximumPopulation[serverId],
            Top5GameModes = top5GameModes[serverId],
            Top5Maps = top5Maps[serverId],
            AveragePopulation = averagePopulation[serverId]
        };
    }
}
