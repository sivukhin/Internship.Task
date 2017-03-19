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

        protected string HttpPrefix = "http://127.0.0.1:12345";

        protected ServerInfo Server1 => new ServerInfo {Id = Host1, Name = "server1", GameModes = new List<string> {ModeA, ModeB}};
        protected ServerInfo Server2 => new ServerInfo {Id = Host2, Name = "server2", GameModes = new List<string> {Mode1, Mode2, Mode3}};
        protected PlayerInfo Player1 => new PlayerInfo {Deaths = 1, Frags = 5, Kills = 5, Name = "player1"};
        protected PlayerInfo Player2 => new PlayerInfo { Deaths = 0, Frags = 10, Kills = 10, Name = "player2" };

        protected MatchInfo Match1 => new MatchInfo
        {
            HostServer = Server1,
            TimeElapsed = 1.0,
            FragLimit = 10,
            GameMode = ModeA,
            Map = "map1",
            Scoreboard = new List<PlayerInfo>
            {
                Player2,
                Player1
            },
            TimeLimit = 10,
            EndTime = DateTime1
        };

        protected MatchInfo Match2 => new MatchInfo
        {
            HostServer = Server2,
            TimeElapsed = 2.0,
            FragLimit = 20,
            GameMode = Mode2,
            Map = "map2",
            Scoreboard = new List<PlayerInfo>
            {
                Player1
            },
            TimeLimit = 40,
            EndTime = DateTime2
        };

        protected MatchInfo GenerateMatch(
            ServerInfo hostServer, 
            DateTime endTime,
            string map = "map1",
            IEnumerable<PlayerInfo> scoreboard = null,
            double timeElapsed = 1.0, 
            int fragLimit = 1, 
            string gameMode = "A",
            int timeLimit = 10)
        {
            if (scoreboard == null)
                scoreboard = new List<PlayerInfo> {Player1};
            return new MatchInfo
            {
                HostServer = hostServer,
                TimeElapsed = timeElapsed,
                FragLimit = fragLimit,
                GameMode = gameMode,
                Map = map,
                Scoreboard = scoreboard.ToList(),
                TimeLimit = timeLimit,
                EndTime = endTime
            };
        }

        protected ServerInfo GenerateServer(
            string endpoint,
            string name = null,
            List<string> gameModes = null)
        {
            if (name == null)
                name = $"server-{endpoint}";
            if (gameModes == null)
                gameModes = new List<string> {"A"};
            return new ServerInfo
            {
                Id = endpoint,
                GameModes = gameModes,
                Name = name
            };
        }

        public IRequest CreateRequest(string content, string route = "", HttpMethodEnum method = HttpMethodEnum.Get)
        {
            var request = A.Fake<IRequest>();
            A.CallTo(() => request.Url).Returns(new Uri(HttpPrefix + route));
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

        protected List<MatchInfo> GenerateMatches(int count, Func<int, MatchInfo> generator)
        {
            var result = new List<MatchInfo>();
            for (int i = 0; i < count; i++)
            {
                var match = generator(i);
                match = match.InitPlayers(match.EndTime);
                result.Add(match);
            }
            return result;
        }

        protected List<ServerInfo> GenerateServers(int count, Func<int, ServerInfo> generator)
        {
            var result = new List<ServerInfo>();
            for (int i = 0; i < count; i++)
            {
                var server = generator(i);
                result.Add(server);
            }
            return result;
        }

        //TODO: description for this function?
        protected async Task WaitForTasks(int timeout = 100)
        {
            await Task.Delay(timeout);
        }
    }
}
    