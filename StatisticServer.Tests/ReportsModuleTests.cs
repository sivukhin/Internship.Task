using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseCore;
using DataCore;
using FakeItEasy;
using FluentAssertions;
using HttpServerCore;
using NHibernate;
using NUnit.Framework;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests
{
    [TestFixture]
    class ReportsModuleTests : BaseModuleTests
    {
        private IServerStatisticStorage serverStatisticStorage = new ServerStatisticStorage();
        private IPlayerStatisticStorage playerStatisticStorage = new PlayerStatisticStorage();
        private IAggregateReportStorage reportStorage;
        private ReportsModule Module => new ReportsModule(reportStorage);

        [SetUp]
        public void SetUp()
        {
            reportStorage = new ReportStorage(serverStatisticStorage, playerStatisticStorage);;
            storage = new SQLiteStorage(DatabaseSessions.CreateSessionFactory(), serverStatisticStorage, playerStatisticStorage, reportStorage);
        }

        [Test]
        public async Task ModuleReturnEmptyArray_WhenNoMatches()
        {
            var response = await Module.ProcessRequest(CreateRequest("", "http://localhost/reports/recent-matches/1"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new MatchInfo[] { }));
        }

        [Test]
        public async Task ModuleReturnRecentMatches()
        {
            await storage.UpdateServerInfo(Server1.ServerId, Server1);
            await storage.UpdateMatchInfo(Server1.ServerId, DateTime1, Match1.InitPlayers());

            var report = ((IReportStorage<MatchInfo>)reportStorage).Report(1);
            report.ToList()[0].Should().Be(Match1);
        }
    }
}
