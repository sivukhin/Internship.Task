using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataCore
{
    public class MatchInfo
    {
        public class MatchInfoId : IComparable<MatchInfoId>
        {
            public string ServerId { get; set; }
            public DateTime EndTime { get; set; }

            public int CompareTo(MatchInfoId other)
            {
                if (ServerId.Equals(other.ServerId))
                    return EndTime.CompareTo(other.EndTime);
                return String.Compare(ServerId, other.ServerId, StringComparison.Ordinal);
            }
        }

        public MatchInfoId GetIndex() => new MatchInfoId {ServerId = HostServer.Id, EndTime = EndTime};

        [JsonIgnore]
        public string Id { get; set; }

        [JsonIgnore]
        public ServerInfo HostServer { get; set; }
        [JsonIgnore]
        public DateTime EndTime { get; set; }

        public string Map { get; set; }
        public string GameMode { get; set; }
        public int FragLimit { get; set; }
        public int TimeLimit { get; set; }
        public double TimeElapsed { get; set; }
        public IList<PlayerInfo> Scoreboard { get; set; }

        public MatchInfo() { }

        public virtual MatchInfo InitPlayers(DateTime endTime)
        {
            EndTime = endTime;
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                Scoreboard[i].BaseMatch = this;
                Scoreboard[i].AreWinner = i == 0;
                Scoreboard[i].ScoreboardPercent = (Scoreboard.Count == 1 ? 100 : 100.0 * i / (Scoreboard.Count - 1));
            }
            return this;
        }

        private bool Equals(MatchInfo other)
        {
            return Equals(HostServer.Id, other.HostServer.Id) && EndTime.Equals(other.EndTime);
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
                var hashCode = HostServer?.Id.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
                return hashCode;
            }
        }
    }
}