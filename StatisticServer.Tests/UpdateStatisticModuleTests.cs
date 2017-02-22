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
    public class UpdateStatisticModuleTests : BaseModuleTests
    {
        protected IStatisticStorage storage;
        protected UpdateStatisticModule Module => new UpdateStatisticModule(storage);

        [SetUp]
        public void SetUp()
        {
            storage = A.Fake<IStatisticStorage>();
            A.CallTo(() => storage.GetMatchInfo(A<string>._, A<DateTime>._)).Returns((MatchInfo)null);
            A.CallTo(() => storage.GetServerInfo(A<string>._)).Returns((ServerInfo)null);
        }

        [Test]
        public async Task ModuleSetNewServerInfo()
        {
            var response = await Module.UpdateServerInfo(CreateRequest(HttpServerExtensions.Serialize(Server1)), Host1);

            response.Should().Be(new HttpResponse(HttpStatusCode.OK));

            A.CallTo(() => storage.UpdateServerInfo(Host1, Server1)).MustHaveHappened();
        }

        [Test]
        public async Task ModuleUpdateServerInfo()
        {
            await Module.UpdateServerInfo(CreateRequest(HttpServerExtensions.Serialize(Server1)), Host1);
            var updateResponse = await Module
                .UpdateServerInfo(CreateRequest(HttpServerExtensions.Serialize(Server2)), Host1);

            updateResponse.Should().Be(new HttpResponse(HttpStatusCode.OK));

            A.CallTo(() => storage.UpdateServerInfo(Host1, Server1)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => storage.UpdateServerInfo(Host1, Server2)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public async Task ModuleReturnBadRequest_WhenUpdateMatch_ForUnknownServer()
        {
            var response = await Module.AddMatchStatistic(CreateRequest(""), Host1, DateTime1);

            response.Should().Be(new HttpResponse(HttpStatusCode.BadRequest));
        }
    }
}
