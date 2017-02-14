using System.Collections.Generic;

namespace Internship.Models
{
    public class MatchInfo
    {
        public string Map { get; set; }
        public string GameMode { get; set; }
        public int FragLimit { get; set; }
        public int TimeLimit { get; set; }
        public double ElapsedTime { get; set; }
        public List<PlayerStatistic> Scoreboard { get; set; }
    }
}