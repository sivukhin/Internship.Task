using System.Threading.Tasks;
using Kontur.GameStats.Server.Modules;
using NUnit.Framework;

namespace Kontur.GameStats.Server.Tests.Modules.ReporstModuleTests
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