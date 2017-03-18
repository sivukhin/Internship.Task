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
        protected IDataStatisticStorage storage;

        protected string Host1 = "host1-1";
        protected string Host2 = "host2-2";

        protected DateTime DateTime1 => new DateTime(2016);
        protected DateTime DateTime2 => new DateTime(2017);

        protected string Mode1 = "1";
        protected string Mode2 = "2";
        protected string Mode3 = "3";

        protected string ModeA = "B";
        protected string ModeB = "A";

        protected ServerInfo Server1 => new ServerInfo {Id = Host1, Name = "server1", GameModes = new List<string> {ModeA, ModeB}};
        protected ServerInfo Server2 => new ServerInfo {Id = Host2, Name = "server2", GameModes = new List<string> {Mode1, Mode2, Mode3}};
        protected PlayerInfo Player1 => new PlayerInfo {Deaths = 1, Frags = 5, Kills = 5, Name = "player1"};
        protected PlayerInfo Player2 => new PlayerInfo { Deaths = 0, Frags = 10, Kills = 10, Name = "player2" };

        protected MatchInfo Match1 => new MatchInfo
        {
            TimeElapsed = 1.0,
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
            TimeElapsed = 2.0,
            FragLimit = 20,
            GameMode = Mode2,
            Map = "map2",
            Scoreboard = new List<PlayerInfo>
            {
                Player1
            },
            TimeLimit = 40
        };

        public IRequest CreateRequest(string content, string uri = "", HttpMethodEnum method = HttpMethodEnum.Get)
        {
            var request = A.Fake<IRequest>();
            A.CallTo(() => request.Url).Returns(new Uri(uri));
            A.CallTo(() => request.Content).Returns(content);
            A.CallTo(() => request.HttpMethod).Returns(method);
            return request;
        }

        public virtual void AddServer(ServerInfo.ServerInfoId serverId, ServerInfo server)
        {
            A.CallTo(() => storage.GetServer(serverId)).Returns(server);
        }

        public void AddMatch(MatchInfo.MatchInfoId matchId, MatchInfo match)
        {
            A.CallTo(() => storage.GetMatch(matchId)).Returns(match);
        }
    }
}
