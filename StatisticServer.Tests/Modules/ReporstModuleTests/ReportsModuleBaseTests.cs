using System.Threading.Tasks;
using NUnit.Framework;
using Raven.Client;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer.Tests
{
    abstract class ReportsMoudleBaseTests : BaseModuleTests
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Module = new ReportsModule(ReportStorage, GlobalStatisticStorage);
            PutInitialiData().Wait();
        }

        protected abstract Task PutInitialiData();
    }
}