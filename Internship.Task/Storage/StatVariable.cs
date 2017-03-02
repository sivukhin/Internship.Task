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
        public GroupStat<string, MatchInfo, int> TotalMatchesPlayed =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Count();

        public GroupStat<string, MatchInfo, int> MaximumPopulation =
            StatFor<MatchInfo>.Group(match => match.HostServer.ServerId).Max(match => match.Scoreboard.Count);
    }
}
