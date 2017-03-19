using System.Net;
using System.Threading.Tasks;
using DataCore;
using FakeItEasy;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests.Modules
{
    [TestFixture]
    public class UpdateStatisticModuleTests : BaseModuleTests
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Module = new UpdateStatisticModule(StatisticStorage);
        }
        [Test]
        public async Task ModuleSetNewServerInfo()
        {
            var response = await Module.ProcessRequest(
                CreateRequest(JsonConvert.SerializeObject(Server1), $"/servers/{Server1.Id}/info", HttpMethodEnum.Put));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            var result = await StatisticStorage.GetServer(Server1.GetIndex());
            result.Should().Be(Server1);
        }
        
        [Test]
        public async Task ModuleUpdateServerInfo()
        {
            await Module.ProcessRequest(
                CreateRequest(JsonConvert.SerializeObject(Server1), $"/servers/{Server1.Id}/info", HttpMethodEnum.Put));
            var response = await Module.ProcessRequest(
                CreateRequest(JsonConvert.SerializeObject(Server2), $"/servers/{Server1.Id}/info", HttpMethodEnum.Put));
            var combined = Server2;
            combined.Id = Server1.Id;

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            var result = await StatisticStorage.GetServer(Server1.GetIndex());
            result.Should().Be(combined);
        }

        [Test]
        public async Task ModuleReturnBadRequest_ForMatchOnUnknownServer()
        {
            var response = await Module.ProcessRequest(CreateRequest(
                JsonConvert.SerializeObject(Match1),
                $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime}",
                HttpMethodEnum.Put));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task ModuleAddNewMatchStatistic()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            var response = await Module.ProcessRequest(CreateRequest(
                JsonConvert.SerializeObject(Match1),
                $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime}",
                HttpMethodEnum.Put));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            var result = await StatisticStorage.GetMatch(Match1.GetIndex());
            result.Should().Be(Match1);
        }

        [Test]
        public async Task ModuleUpdateMatchStatistic()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await Module.ProcessRequest(CreateRequest(
                JsonConvert.SerializeObject(Match1),
                $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime}",
                HttpMethodEnum.Put));
            var response = await Module.ProcessRequest(CreateRequest(
                JsonConvert.SerializeObject(Match2),
                $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime}",
                HttpMethodEnum.Put));
            var combined = Match2;
            combined.HostServer = Match1.HostServer;
            combined.EndTime = Match1.EndTime;

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            var result = await StatisticStorage.GetMatch(Match1.GetIndex());
            result.ShouldBeEquivalentTo(combined,
                options => options
                    .Excluding(info => info.Id)
                    .Excluding(info => info.Scoreboard));
        }
    }
}
