using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using HttpServerCore;
using NUnit.Framework;
using StatisticServer.Models;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests
{
    public abstract class BaseModuleTests
    {
        protected IStatisticStorage storage;

        protected string Host1 = "host1-1";
        protected string Host2 = "host2-2";

        protected DateTime DateTime1 => new DateTime(2016);
        protected DateTime DateTime2 => new DateTime(2017);

        protected ServerInfo Server1 => new ServerInfo {Name = "server1", GameModes = new List<string> {"A", "B"}};
        protected ServerInfo Server2 = new ServerInfo {Name = "server2", GameModes = new List<string> {"1", "2", "3"}};
        protected PlayerStatistic Player1 => new PlayerStatistic {Deaths = 1, Frags = 5, Kills = 5, Name = "player1"};
        protected PlayerStatistic Player2 => new PlayerStatistic { Deaths = 0, Frags = 10, Kills = 10, Name = "player2" };

        protected MatchInfo Match1 => new MatchInfo
        {
            ElapsedTime = 1.0,
            FragLimit = 10,
            GameMode = "A",
            Map = "map1",
            Scoreboard = new List<PlayerStatistic>
            {
                Player2,
                Player1
            },
            TimeLimit = 10
        };

        protected MatchInfo Match2 => new MatchInfo
        {
            ElapsedTime = 2.0,
            FragLimit = 20,
            GameMode = "2",
            Map = "map2",
            Scoreboard = new List<PlayerStatistic>
            {
                Player1
            },
            TimeLimit = 40
        };

        public IRequest CreateRequest(string content)
        {
            var request = A.Fake<IRequest>();
            A.CallTo(() => request.Content).Returns(content);
            return request;
        }

        public virtual void AddServer(string serverId, ServerInfo serverInfo)
        {
            A.CallTo(() => storage.GetServerInfo(serverId)).Returns(serverInfo);
        }

        public void AddMatch(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            A.CallTo(() => storage.GetMatchInfo(serverId, endTime)).Returns(matchInfo);
        }
    }
}
