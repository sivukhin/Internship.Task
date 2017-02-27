using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FakeItEasy;
using HttpServerCore;
using NUnit.Framework;
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

        protected GameMode Mode1 => new GameMode("1");
        protected GameMode Mode2 => new GameMode("2");
        protected GameMode Mode3 => new GameMode("3");

        protected GameMode ModeA => new GameMode("A");
        protected GameMode ModeB => new GameMode("B");

        protected ServerInfo Server1 => new ServerInfo {ServerId = Host1, Name = "server1", GameModes = new List<GameMode> {ModeA, ModeB}};
        protected ServerInfo Server2 => new ServerInfo {ServerId = Host2, Name = "server2", GameModes = new List<GameMode> {Mode1, Mode2, Mode3}};
        protected PlayerInfo Player1 => new PlayerInfo {Deaths = 1, Frags = 5, Kills = 5, Name = "player1"};
        protected PlayerInfo Player2 => new PlayerInfo { Deaths = 0, Frags = 10, Kills = 10, Name = "player2" };

        protected MatchInfo Match1 => new MatchInfo
        {
            ElapsedTime = 1.0,
            FragLimit = 10,
            GameMode = ModeA,
            Map = "map1",
            Scoreboard = new List<PlayerInfo>
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
            GameMode = Mode2,
            Map = "map2",
            Scoreboard = new List<PlayerInfo>
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
