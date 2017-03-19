using System.Net;
using System.Threading.Tasks;
using DataCore;
using FakeItEasy;
using FluentAssertions;
using HttpServerCore;
using NUnit.Framework;
using Raven.Imports.Newtonsoft.Json;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests.Modules
{
    [TestFixture]
    public class UpdateStatisticModuleTests : BaseModuleTests
    {
        /*
        [Test]
        public async Task ModuleSetNewServerInfo()
        {
            var response = await Module.UpdateServerInfo(CreateRequest(JsonConvert.SerializeObject(Server1)), Host1);

            response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            A.CallTo(() => storage.UpdateServer(new ServerInfo.ServerInfoId {Id = Host1}, Server1)).MustHaveHappened();
        }

        [Test]
        public async Task ModuleUpdateServerInfo()
        {
            await Module.UpdateServerInfo(CreateRequest(JsonConvert.SerializeObject(Server1)), Host1);
            var updateResponse = await Module
                .UpdateServerInfo(CreateRequest(JsonConvert.SerializeObject(Server2)), Host1);

            updateResponse.Should().Be(new HttpResponse(HttpStatusCode.OK));

            A.CallTo(() => storage.UpdateServer(new ServerInfo.ServerInfoId {Id = Host1}, Server1))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => storage.UpdateServer(new ServerInfo.ServerInfoId {Id = Host1}, Server2))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task ModuleReturnBadRequest_ForMatchOnUnknownServer()
        {
            var response = await Module.AddMatchStatistic(CreateRequest(""), Host1, DateTime1);

            response.Should().Be(new HttpResponse(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task ModuleAddNewMatchStatistic()
        {
            AddServer(new ServerInfo.ServerInfoId {Id = Host1}, Server1);
            var response = await Module
                .AddMatchStatistic(CreateRequest(JsonConvert.SerializeObject(Server1)), Host1, DateTime1);

            A.CallTo(() => storage.UpdateMatch(new MatchInfo.MatchInfoId {ServerId = Host1, EndTime = DateTime1}, A<MatchInfo>._))
                .MustHaveHappened();
            response.Should().Be(new HttpResponse(HttpStatusCode.OK));
        }*/
    }
}
