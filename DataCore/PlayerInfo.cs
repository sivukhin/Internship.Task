using System.Runtime.Serialization;
using Newtonsoft.Json;
using NHibernate.Util;

namespace DataCore
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int Frags { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public MatchInfo BaseMatch { get; set; }
        [JsonIgnore]
        public double ScoreboardPercent { get; set; }
        [JsonIgnore]
        public bool AreWinner { get;set; }

        private bool Equals(PlayerInfo other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PlayerInfo)obj);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}