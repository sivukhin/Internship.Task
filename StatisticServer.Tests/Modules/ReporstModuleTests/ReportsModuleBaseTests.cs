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
        protected IGlobalServerStatisticStorage GlobalStatisticStorage;
        protected IServerStatisticStorage ServerStatisticStorage;
        protected IPlayerStatisticStorage PlayerStatisticStorage;
        protected IReportStorage ReportStorage;
        protected IDataRepository DataRepository;
        protected IDataStatisticStorage StatisticStorage;
        protected IDocumentStore DocumentStore;
        protected ReportsModule module;

        [SetUp]
        public async Task Setup()
        {
            GlobalStatisticStorage = new GlobalServerStatisticStorage();
            ServerStatisticStorage = new ServerStatisticStorage(GlobalStatisticStorage);
            PlayerStatisticStorage = new PlayerStatisticStorage();
            ReportStorage = new ReportStorage(ServerStatisticStorage, PlayerStatisticStorage, GlobalStatisticStorage);
            DocumentStore = RavenDbStore.GetStore(new ApplicationOptions
            {
                InMemory = true,
                UnitTesting = true
            });
            DataRepository = new RavenDbStorage(DocumentStore);
            StatisticStorage = new DataStatisticStorage(
                DataRepository,
                GlobalStatisticStorage,
                ServerStatisticStorage,
                PlayerStatisticStorage,
                ReportStorage);
            module = new ReportsModule(ReportStorage, GlobalStatisticStorage);

            await PutInitialiData();
        }

        protected abstract Task PutInitialiData();
    }
}