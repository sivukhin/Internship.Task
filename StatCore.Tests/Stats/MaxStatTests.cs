using NUnit.Framework;
using StatCore.Stats;

namespace StatCore.Tests.Stats
{
    [TestFixture]
    class MaxStatTests : BaseStatTest
    {
        private IStat<int, int> stat;

        [SetUp]
        public void SetUp()
        {
            stat = new MinMaxStat<int, int>(i => i).Select(minMax => minMax.Item2);
        }

        private static readonly TestCaseData[] maxStatTests = {
            ParamsTestCaseData.Create<Event<int>>()
                .Returns(0).SetName("ReturnDefault_WhenNoItemsAdded"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(3))
                .Returns(3).SetName("AddItems"),
            ParamsTestCaseData.Create(Event<int>.Add(3), Event<int>.Add(2))
                .Returns(3).SetName("AddItems"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Delete(2))
                .Returns(1).SetName("DeleteItems"),
            ParamsTestCaseData.Create(Event<int>.Add(10), Event<int>.Delete(10))
                .Returns(0).SetName("ReturnDefault_WhenAllElementsDeleted")
        };

        [TestCaseSource(nameof(maxStatTests))]
        public int MaxStat_Should(Event<int>[] events)
        {
            HandleEvents(stat, events);
            return stat.Value;
        }
    }
}
