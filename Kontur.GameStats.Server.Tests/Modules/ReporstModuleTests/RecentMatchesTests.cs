using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;

namespace Kontur.GameStats.Server.Tests.Modules.ReporstModuleTests
{
    [TestFixture]
    class RecentMatchesReportsTests : ReportsMoudleBaseTests
    {
        [Test]
        public async Task RecentMatches_ReturnAllMatches_IfLessThanCount()
        {
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/5"));

            var expected = new[]
            {
                new {server = Match1.HostServer.Id, timestamp = Match1.EndTime, results = Match1},
            }.OrderByDescending(m => m.timestamp).ToArray();
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_ReturnEmptyCollection_IfInvalidCountValue()
        {
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/one"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task RecentMatches_ReturnNotMoreThanCountEntries()
        {
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await StatisticStorage.UpdateMatch(Match2.GetIndex(), Match2.InitPlayers(Match2.EndTime));
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/1"));

            var expected = new[]
            {
                new { server = Match1.HostServer.Id, timestamp = Match1.EndTime, results=Match1},
                new { server = Match2.HostServer.Id, timestamp = Match2.EndTime, results=Match2},
            }.OrderByDescending(m => m.timestamp).Take(1).ToArray();
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_DefaultCountValue_Is5()
        {
            foreach (var match in GenerateMatches(10, i => GenerateMatch(Server1, new DateTime(i))))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await Task.Delay(100);

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches"));
            var matches = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            matches.Should().HaveCount(5);
        }

        [Test]
        public async Task RecentMatches_AcceptNonCanonicalRoute()
        {
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/"));

            var expected = new[]
            {
                new {server = Match1.HostServer.Id, timestamp = Match1.EndTime, results = Match1}
            };
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_MinimumCountValue_Is0()
        {
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/-1"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task RecentMatches_MaximumCountValue_Is50()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i))))
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", "/reports/recent-matches/100"));

            var matches = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            matches.Should().HaveCount(50);
        }

        [Test]
        public async Task RecentMatches_ReturnRecentMatches()
        {
            int totalCount = 100, queryCount = 7;
            var random = new Random(0);
            var generated = GenerateMatches(totalCount, i => GenerateMatch(
                Server1, new DateTime(random.Next(100000)), $"map{i + 1}"));
            foreach (var match in generated)
            {
                await StatisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await WaitForTasks();

            var response = await Module.ProcessRequest(CreateRequest("", $"/reports/recent-matches/{queryCount}"));

            var expected = generated
                .Select(m => new {server = m.HostServer.Id, timestamp = m.EndTime, results = m})
                .OrderByDescending(m => m.timestamp).Take(queryCount).ToArray();
            
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        protected override async Task PutInitialiData()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
        }
    }
}
