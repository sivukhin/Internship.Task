using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework.Internal;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;
using Raven.Tests.Helpers;

namespace StatisticServer.Tests.Modules.ReporstModuleTests
{
    [TestFixture]
    class PopularServersTests : ReportsMoudleBaseTests
    {
        [Test]
        public async Task PopularServers_ReturnAllServers_IfLessThanCount()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/5"));
            var typeDefinition = new {endpoint = "", name = "", averageMatchesPerDay = 0.0};
            var expected = new[] {Server1, Server2}.Select(s => new {endpoint = s.Id, name = s.Name, averageMatchesPerDay = 0.0});
            var servers = JsonConvert.DeserializeObject<List<JObject>>(response.Response.Content)
                .Select(value => value.ToObject(typeDefinition.GetType()));

            servers.ShouldBeEquivalentTo(expected);
        }

        [Test]
        public async Task PopularServers_ReturnMostPopularServer()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            RavenTestBase.WaitForIndexing(DocumentStore);
            await StatisticStorage.UpdateMatch(Match2.GetIndex(), Match2.InitPlayers(Match2.EndTime));
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/1"));
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new[]
            {
                new
                {
                    endpoint = Server2.Id,
                    name = Server2.Name,
                    averageMatchesPerDay = 1.0,
                }
            }));
        }

        [Test]
        public async Task PopularServers_DefaultCountParameter_Is5()
        {
            foreach (var server in GenerateServers(10, i => GenerateServer(i.ToString())))
                await StatisticStorage.UpdateServer(server.GetIndex(), server);
            RavenTestBase.WaitForIndexing(DocumentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers"));
            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(5);
        }

        [Test]
        public async Task PopularServers_AcceptNonCanonicalRoute()
        {
            foreach (var server in GenerateServers(10, i => GenerateServer(i.ToString())))
                await StatisticStorage.UpdateServer(server.GetIndex(), server);
            RavenTestBase.WaitForIndexing(DocumentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/"));
            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(5);
        }

        [Test]
        public async Task PopularServers_MinimalCountValue_Is0()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            RavenTestBase.WaitForIndexing(DocumentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/-1"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] { }));
        }

        [Test]
        public async Task PopularServers_MaximumCountParameter_Is50()
        {
            foreach (var server in GenerateServers(100, i => GenerateServer(i.ToString())))
                await StatisticStorage.UpdateServer(server.GetIndex(), server);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/100"));

            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(50);
        }

        [Test]
        public async Task PopularServers_ReturnEmptyCollection_IfInvalidCountValue()
        {
            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/one"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }

        [Test]
        public async Task PopularServers_CalculateAverageMatchesPerDay()
        {
            foreach (var server in GenerateServers(100, i => GenerateServer(i.ToString())))
                await StatisticStorage.UpdateServer(server.GetIndex(), server);
            RavenTestBase.WaitForIndexing(DocumentStore);
            var match1 = GenerateMatch(GenerateServer("1"), DateTime.Today);
            var match2 = GenerateMatch(GenerateServer("1"), DateTime.Today.Subtract(TimeSpan.FromDays(1)));
            var match3 = GenerateMatch(GenerateServer("2"), DateTime.Today.Add(TimeSpan.FromDays(2)));
            await StatisticStorage.UpdateMatch(match1.GetIndex(), match1.InitPlayers(match1.EndTime));
            await StatisticStorage.UpdateMatch(match2.GetIndex(), match2.InitPlayers(match2.EndTime));
            await StatisticStorage.UpdateMatch(match3.GetIndex(), match3.InitPlayers(match3.EndTime));
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/2"));
            var expected = new[]
            {
                new
                {
                    endpoint = match1.HostServer.Id,
                    name = match1.HostServer.Name,
                    averageMatchesPerDay = 0.5
                },
                new
                {
                    endpoint = match3.HostServer.Id,
                    name = match3.HostServer.Name,
                    averageMatchesPerDay = 0.25
                },
            };
            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, expected));
        }


        protected override async Task PutInitialiData() { }
    }
}
