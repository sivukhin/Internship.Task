using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentNHibernate.Utils;
using HttpServerCore;
using NUnit.Framework;
using Raven.Client;
using Raven.Tests.Helpers;
using StatisticServer.Modules;
using StatisticServer.Storage;
using Raven.Imports.Newtonsoft.Json;

namespace StatisticServer.Tests
{
    [TestFixture]
    class RecentMatchesReportsTests : BaseModuleTests
    {
        private IServerStatisticStorage serverStatisticStorage;
        private IPlayerStatisticStorage playerStatisticStorage;
        private IReportStorage reportStorage;
        private IDataRepository dataRepository;
        private IDataStatisticStorage statisticStorage;
        private IDocumentStore documentStore;
        private ReportsModule module;

        [SetUp]
        public async Task Setup()
        {
            serverStatisticStorage = new ServerStatisticStorage();
            playerStatisticStorage = new PlayerStatisticStorage();
            reportStorage = new ReportStorage(serverStatisticStorage, playerStatisticStorage);
            documentStore = RavenDbStore.GetStore(new ApplicationOptions
            {
                InMemory = true,
                UnitTesting = true
            });
            dataRepository = new RavenDbStorage(documentStore);
            statisticStorage = new DataStatisticStorage(
                dataRepository,
                serverStatisticStorage,
                playerStatisticStorage,
                reportStorage);
            module = new ReportsModule(reportStorage);

            await statisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await statisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            RavenTestBase.WaitForIndexing(documentStore);
        }

        [Test]
        public async Task RecentMatches_ReturnSingleMatch()
        {
            await statisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches"));

            var expected = new[]
            {
                new {server = Match1.HostServer.Id, timestamp = Match1.EndTime, results = Match1},
            }.OrderByDescending(m => m.timestamp).ToArray();
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_ReturnEmptyCollection_IfInvalidCountValue()
        {
            await statisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches/one"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task RecentMatches_ReturnNotMoreThanCountEntries()
        {
            await statisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await statisticStorage.UpdateMatch(Match2.GetIndex(), Match2.InitPlayers(Match2.EndTime));
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches/1"));

            var expected = new[]
            {
                new { server = Match1.HostServer.Id, timestamp = Match1.EndTime, results=Match1},
                new { server = Match2.HostServer.Id, timestamp = Match2.EndTime, results=Match2},
            }.OrderByDescending(m => m.timestamp).Take(1).ToArray();
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_DefaultCountParameter_Is5()
        {
            foreach (var match in GenerateMatches(10, i => GenerateMatch(Server1, new DateTime(i))))
            {
                await statisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches"));
            var matches = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            matches.Should().HaveCount(5);
        }

        [Test]
        public async Task RecentMatches_AcceptNonCanonicalRoute()
        {
            await statisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches/"));

            var expected = new[]
            {
                new {server = Match1.HostServer.Id, timestamp = Match1.EndTime, results = Match1}
            };
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }

        [Test]
        public async Task RecentMatches_MinimumCountParameter_Is0()
        {
            await statisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches/-1"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task RecentMatches_MaximumCountParameter_Is50()
        {
            foreach (var match in GenerateMatches(100, i => GenerateMatch(Server1, new DateTime(i))))
            {
                await statisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", "/reports/recent-matches/100"));

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
                await statisticStorage.UpdateMatch(match.GetIndex(), match.InitPlayers(match.EndTime));
            }
            await Task.Delay(100);

            var response = await module.ProcessRequest(CreateRequest("", $"/reports/recent-matches/{queryCount}"));

            var expected = generated
                .Select(m => new {server = m.HostServer.Id, timestamp = m.EndTime, results = m})
                .OrderByDescending(m => m.timestamp).Take(queryCount).ToArray();
            
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }
    }
}
