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
