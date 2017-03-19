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
            await statisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await statisticStorage.UpdateServer(Server2.GetIndex(), Server2);
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
            await statisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await statisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            RavenTestBase.WaitForIndexing(documentStore);
            await statisticStorage.UpdateMatch(Match2.GetIndex(), Match2.InitPlayers(Match2.EndTime));
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
                await statisticStorage.UpdateServer(server.GetIndex(), server);
            RavenTestBase.WaitForIndexing(documentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers"));
            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(5);
        }

        [Test]
        public async Task PopularServers_AcceptNonCanonicalRoute()
        {
            foreach (var server in GenerateServers(10, i => GenerateServer(i.ToString())))
                await statisticStorage.UpdateServer(server.GetIndex(), server);
            RavenTestBase.WaitForIndexing(documentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/"));
            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(5);
        }

        [Test]
        public async Task PopularServers_MinimalCountValue_Is0()
        {
            await statisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            RavenTestBase.WaitForIndexing(documentStore);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/-1"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] { }));
        }

        [Test]
        public async Task RecentMatches_MaximumCountParameter_Is50()
        {
            foreach (var server in GenerateServers(100, i => GenerateServer(i.ToString())))
                await statisticStorage.UpdateServer(server.GetIndex(), server);
            await WaitForTasks();

            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/100"));

            var servers = JsonConvert.DeserializeObject<List<object>>(response.Response.Content);
            servers.Should().HaveCount(50);
        }

        [Test]
        public async Task RecentMatches_ReturnEmptyCollection_IfInvalidCountValue()
        {
            var response = await module.ProcessRequest(CreateRequest("", "/reports/popular-servers/one"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new object[] {}));
        }


        protected override async Task PutInitialiData() { }
    }
}
