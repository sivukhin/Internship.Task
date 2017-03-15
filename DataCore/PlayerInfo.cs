using Newtonsoft.Json;
using NHibernate.Util;

namespace DataCore
{
    public class PlayerInfo
    {
        public virtual int PlayerId { get; set; }
        public virtual string Name { get; set; }
        public virtual int Frags { get; set; }
        public virtual int Kills { get; set; }
        public virtual int Deaths { get; set; }

        [JsonIgnore]
        public virtual MatchInfo BaseMatch { get; set; }
        [JsonIgnore]
        public virtual double ScoreboardPercent { get; set; }
        [JsonIgnore]
        public virtual bool AreWinner => BaseMatch.Scoreboard.First() == this;

        private bool Equals(PlayerInfo other)
        {
            return PlayerId == other.PlayerId &&
                   string.Equals(Name, other.Name) &&
                   Frags == other.Frags &&
                   Kills == other.Kills &&
                   Deaths == other.Deaths;
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
            unchecked
            {
                var hashCode = PlayerId;
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Frags;
                hashCode = (hashCode * 397) ^ Kills;
                hashCode = (hashCode * 397) ^ Deaths;
                return hashCode;
            }
        }
    }
}