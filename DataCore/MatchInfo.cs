using System;
using System.Collections.Generic;

namespace DataCore
{
    public class MatchInfo
    {
        public virtual int MatchId { get; set; }
        public virtual ServerInfo HostServer { get; set; }
        public virtual string Map { get; set; }
        public virtual GameMode GameMode { get; set; }
        public virtual int FragLimit { get; set; }
        public virtual int TimeLimit { get; set; }
        public virtual double ElapsedTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual IList<PlayerInfo> Scoreboard { get; set; }
    }
}