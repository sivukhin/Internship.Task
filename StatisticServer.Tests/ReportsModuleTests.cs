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
        private ReportStorage reportStorage;
        private ReportsModule Module => new ReportsModule(reportStorage);
    }
}
