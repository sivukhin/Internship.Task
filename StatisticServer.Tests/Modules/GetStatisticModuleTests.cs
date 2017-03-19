using System.Net;
using System.Threading.Tasks;
using DataCore;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using Raven.Client;
using Raven.Tests.Helpers;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests.Modules
{
    [TestFixture]
    public class GetStatisticModuleTests : BaseModuleTests
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Module = new GetStatisticModule(StatisticStorage);
        }

        [Test]
        public async Task ModuleReturnsEmptyArray_WhenNoServersRegistred()
        {
            var response = await Module.ProcessRequest(CreateRequest("", "/servers/info"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new ServerInfo[] {}));
        }

        [Test]
        public async Task ModuleReturnsAllServerInfos()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            RavenTestBase.WaitForIndexing(DocumentStore);

            var response = await Module.ProcessRequest(CreateRequest("", "/servers/info"));

            response.Response.ShouldBeEquivalentTo(new JsonHttpResponse(HttpStatusCode.OK, new[]
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
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/info"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsServerInfo()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            RavenTestBase.WaitForIndexing(DocumentStore);

            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/info"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Server1));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoMatchesFound()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            RavenTestBase.WaitForIndexing(DocumentStore);

            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/matches/{DateTime1}"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoServersFound()
        {
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Host1}/matches/{DateTime1}"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsMatchInfo()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            RavenTestBase.WaitForIndexing(DocumentStore);
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            RavenTestBase.WaitForIndexing(DocumentStore);
            
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime}"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Match1));
        }
    }
}
