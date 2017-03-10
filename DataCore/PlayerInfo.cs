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
        public virtual MatchInfo BaseMatch { get; set; }

        public virtual double ScoreboardPercent { get; set; }

        public virtual bool AreWinner => BaseMatch.Scoreboard.First() == this;
    }
}