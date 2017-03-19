using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using StatisticServer.Storage;

namespace StatisticServer.Tests.Modules.ReporstModuleTests
{
    class BestPlayersTests : ReportsMoudleBaseTests
    {
        [Test]
        public async Task BestPlayers_IgnorePlayers_WithLessThan_10_Matches()
        {
            foreach (var match in GenerateMatches(9, i => GenerateMatch(Server1, new DateTime(i), "map", new[] {Player1})))
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players"));
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task BestPlayers_ReturnSinglePlayer()
        {
            foreach (var match in GenerateMatches(10, i => GenerateMatch(Server1, new DateTime(i), "map", new[] { Player1 })))
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players"));
            var expected = new[]
            {
                new
                {
                    name = Player1.Name,
                    killToDeathRatio = 1.0 * Player1.Kills / Player1.Deaths
                }
            };
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task BestPlayers_IgnorePlayers_WhoNeverDie()
        {
            foreach (var match in GenerateMatches(10, i => GenerateMatch(Server1, new DateTime(i), "map", new[] { Player2 })))
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players"));
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task BestPlayers_AcceptNonCanonicalRoute()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i), "map",
                new[] {GeneratePlayer((i / 10).ToString())})))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players/"));
            var players = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            players.Should().HaveCount(5);
        }

        [Test]
        public async Task BestPlayers_DefaultCountValue_Is5()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i), "map",
                new[] { GeneratePlayer((i / 10).ToString()) })))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players"));
            var players = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            players.Should().HaveCount(5);
        }

        [Test]
        public async Task BestPlayers_MaximumCountValue_Is50()
        {
            foreach (var match in GenerateMatches(1000, i => GenerateMatch(Server1, new DateTime(i), "map",
                new[] { GeneratePlayer((i / 10).ToString()) })))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players/100"));
            var players = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            players.Should().HaveCount(50);
        }

        [Test]
        public async Task BestPlayers_MinimumCountValue_Is0()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i), "map",
                new[] { GeneratePlayer((i / 10).ToString()) })))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players/-1"));
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task BestPlayers_ReturnAllPlayers_IfLessThanCount()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i), "map",
                new[] { GeneratePlayer((i / 10).ToString()) })))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            var response = await module.ProcessRequest(CreateRequest("", "/reports/best-players/20"));
            var players = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            players.Should().HaveCount(10);
        }

        protected override async Task PutInitialiData()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            RavenTestBase.WaitForIndexing(DocumentStore);
        }
    }
}
