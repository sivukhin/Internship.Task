using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataCore
{
    public class MatchInfo
    {
        public int MatchId { get; set; }
        public ServerInfo HostServer { get; set; }
        public string Map { get; set; }
        public string GameMode { get; set; }
        public int FragLimit { get; set; }
        public int TimeLimit { get; set; }
        public double ElapsedTime { get; set; }
        public DateTime EndTime { get; set; }
        public IList<PlayerInfo> Scoreboard { get; set; }

        public MatchInfo() { }

        public virtual MatchInfo InitPlayers()
        {
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                Scoreboard[i].BaseMatch = this;
                Scoreboard[i].ScoreboardPercent = (Scoreboard.Count == 1 ? 100 : 100.0 * i / (Scoreboard.Count - 1));
            }
            return this;
        }

        private bool Equals(MatchInfo other)
        {
            return MatchId == other.MatchId && Equals(HostServer, other.HostServer) && string.Equals(Map, other.Map) &&
                   Equals(GameMode, other.GameMode) && FragLimit == other.FragLimit && TimeLimit == other.TimeLimit &&
                   ElapsedTime.Equals(other.ElapsedTime) && EndTime.Equals(other.EndTime) &&
                   Scoreboard.SequenceEqual(other.Scoreboard);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MatchId;
                hashCode = (hashCode * 397) ^ (HostServer?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Map?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (GameMode?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ FragLimit;
                hashCode = (hashCode * 397) ^ TimeLimit;
                hashCode = (hashCode * 397) ^ ElapsedTime.GetHashCode();
                hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (Scoreboard?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}