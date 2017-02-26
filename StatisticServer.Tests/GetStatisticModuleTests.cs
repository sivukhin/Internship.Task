using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using StatisticServer.Models;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests
{
    [TestFixture]
    public class GetStatisticModuleTests : BaseModuleTests
    {
        private List<ServerInfo> registredServers;
        private GetStatisticModule Module => new GetStatisticModule(storage);

        [SetUp]
        public void SetUp()
        {
            storage = A.Fake<IStatisticStorage>();
            registredServers = new List<ServerInfo>();

            A.CallTo(() => storage.GetAllServersInfo()).Returns(registredServers);
            A.CallTo(() => storage.GetMatchInfo(A<string>._, A<DateTime>._)).Returns((MatchInfo)null);
            A.CallTo(() => storage.GetServerInfo(A<string>._)).Returns((ServerInfo)null);
        }

        public override void AddServer(string serverId, ServerInfo serverInfo)
        {
            A.CallTo(() => storage.GetServerInfo(serverId)).Returns(serverInfo);
            registredServers.Add(serverInfo);
        }

        [Test]
        public async Task ModuleReturnsEmptyArray_WhenNoServersRegistred()
        {
            var response = await Module.GetAllServersInfo();

            response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new ServerInfo[] {}));
        }

        [Test]
        public async Task ModuleReturnsAllServerInfos()
        {
            AddServer(Host1, Server1);
            AddServer(Host2, Server2);

            var response = await Module.GetAllServersInfo();

            response.ShouldBeEquivalentTo(new JsonHttpResponse(HttpStatusCode.OK, new[] {Server1, Server2}));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenServerNotRegistred()
        {
            var response = await Module.GetServerInfo(Host1);

            response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsServerInfo()
        {
            AddServer(Host1, Server1);

            var response = await Module.GetServerInfo(Host1);

            response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Server1));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoMatchesFound()
        {
            AddServer(Host1, Server1);

            var response = await Module.GetMatchInfo(Host1, DateTime1);

            response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoServersFound()
        {
            var response = await Module.GetMatchInfo(Host1, DateTime1);

            response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }
    }
}
