using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DataCore
{
    public class MatchInfo
    {
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

        public MatchInfoId GetIndex() => new MatchInfoId {ServerId = HostServer.Id, EndTime = EndTime};

        private double ScoreboardPercentCalculator(int place)
        {
            if (Scoreboard.Count == 1)
                return 100;
            double percentBelow = 1 - place * 1.0 / (Scoreboard.Count - 1);
            return 100 * percentBelow;
        }

        public virtual MatchInfo InitPlayers(DateTime endTime)
        {
            EndTime = endTime;
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                Scoreboard[i].BaseMatch = this;
                Scoreboard[i].AreWinner = i == 0;
                Scoreboard[i].ScoreboardPercent = ScoreboardPercentCalculator(i);
            }
            return this;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MatchInfo)obj);
        }

        public override int GetHashCode()
        {
            return GetIndex().GetHashCode();
        }

        private bool Equals(MatchInfo other)
        {
            return GetIndex().Equals(other.GetIndex());
        }

        public class MatchInfoId : IComparable<MatchInfoId>
        {
            public string ServerId { get; set; }
            public DateTime EndTime { get; set; }

            public int CompareTo(MatchInfoId other)
            {
                if (ServerId.Equals(other.ServerId))
                    return EndTime.CompareTo(other.EndTime);
                return string.Compare(ServerId, other.ServerId, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((MatchInfoId)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((ServerId?.GetHashCode() ?? 0) * 397) ^ EndTime.GetHashCode();
                }
            }

            protected bool Equals(MatchInfoId other)
            {
                return string.Equals(ServerId, other.ServerId) && EndTime.Equals(other.EndTime);
            }
        }
    }
}