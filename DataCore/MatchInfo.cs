using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        public MatchInfo() { }

        [JsonConstructor]
        public MatchInfo(int matchId, string map, string gameMode, int fragLimit, int timeLimit, double elapsedTime, IList<PlayerInfo> scoreboard)
        {
            MatchId = matchId;
            Map = map;
            GameMode = new GameMode(gameMode);
            FragLimit = fragLimit;
            TimeLimit = timeLimit;
            ElapsedTime = elapsedTime;
            Scoreboard = scoreboard;
        }

    }
}