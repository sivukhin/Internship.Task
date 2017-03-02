using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using StatCore;

namespace StatisticServer.Storage
{
    public class ServerStatistic
    {
        public GroupStat<MatchInfo, int, string> TotalMatchesPlayed =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Count();

        public GroupStat<MatchInfo, int, string> MaximumMatchesPerDay =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .MaxByGroup(match => match.EndTime.Date, () => StatFor<MatchInfo>.Count());

        public GroupStat<MatchInfo, double, string> AverageMatchesPerDay =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .AverageByGroup(match => match.EndTime.Date, () => StatFor<MatchInfo>.Count().Select(i => (double)i));

        public GroupStat<MatchInfo, int, string> MaximumPopulation =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Max(match => match.Scoreboard.Count);

        public GroupStat<MatchInfo, double, string> AveragePopulation =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Average(match => match.Scoreboard.Count);

        public GroupStat<MatchInfo, IEnumerable<string>, string> Top5GameModes =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .Popular(5, match => match.GameMode.ModeName);

        public GroupStat<MatchInfo, IEnumerable<string>, string> Top5Maps =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId)
                .Popular(5, match => match.Map);

    }
}
