using System.Threading.Tasks;
using NUnit.Framework;
using Raven.Client;
using Raven.Tests.Helpers;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests
{
    abstract class ReportsMoudleBaseTests : BaseModuleTests
    {
        protected IServerStatisticStorage serverStatisticStorage;
        protected IPlayerStatisticStorage playerStatisticStorage;
        protected IReportStorage reportStorage;
        protected IDataRepository dataRepository;
        protected IDataStatisticStorage statisticStorage;
        protected IDocumentStore documentStore;
        protected ReportsModule module;

        [SetUp]
        public async Task Setup()
        {
            serverStatisticStorage = new ServerStatisticStorage();
            playerStatisticStorage = new PlayerStatisticStorage();
            reportStorage = new ReportStorage(serverStatisticStorage, playerStatisticStorage);
            documentStore = RavenDbStore.GetStore(new ApplicationOptions
            {
                InMemory = true,
                UnitTesting = true
            });
            dataRepository = new RavenDbStorage(documentStore);
            statisticStorage = new DataStatisticStorage(
                dataRepository,
                serverStatisticStorage,
                playerStatisticStorage,
                reportStorage);
            module = new ReportsModule(reportStorage);

            await PutInitialiData();
        }

        protected abstract Task PutInitialiData();
    }
}