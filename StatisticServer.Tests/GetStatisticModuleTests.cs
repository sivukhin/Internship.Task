using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FakeItEasy;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
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

            A.CallTo(() => storage.GetAllServers()).Returns(registredServers);
            A.CallTo(() => storage.GetMatch(A<MatchInfo.MatchInfoId>._)).Returns((MatchInfo)null);
            A.CallTo(() => storage.GetServer(A<ServerInfo.ServerInfoId>._)).Returns((ServerInfo)null);
        }

        public override void AddServer(ServerInfo.ServerInfoId serverId, ServerInfo server)
        {
            A.CallTo(() => storage.GetServer(serverId)).Returns(server);
            registredServers.Add(server);
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
            AddServer(new ServerInfo.ServerInfoId {Id = Host1}, Server1);
            AddServer(new ServerInfo.ServerInfoId {Id = Host2}, Server2);

            var response = await Module.GetAllServersInfo();

            response.ShouldBeEquivalentTo(new JsonHttpResponse(HttpStatusCode.OK, new[]
            {
                new
                {
                    endpoint = Server1.Id,
                    info = Server1
                },
                new
                {
                    endpoint = Server2.Id,
                    info = Server2
                }
            }));
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
            AddServer(new ServerInfo.ServerInfoId {Id = Host1}, Server1);

            var response = await Module.GetServerInfo(Host1);

            response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Server1));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoMatchesFound()
        {
            AddServer(new ServerInfo.ServerInfoId { Id = Host1 }, Server1);

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
