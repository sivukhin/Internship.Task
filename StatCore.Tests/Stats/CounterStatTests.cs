using FluentAssertions;
using NUnit.Framework;
using StatCore.Stats;

namespace StatCore.Tests.Stats
{
    [TestFixture]
    public class CounterStatTests : BaseStatTest
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void CounterStat_ShouldCountFromInitialStat(int initialStat)
        {
            var stat = new CounterStat<int>(initialStat);
            stat.Value.Should().Be(initialStat);
        }

        private static readonly TestCaseData[] counterStatTests = {
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1))
                .Returns(2).SetName("CountAddedItems"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Delete(1))
                .Returns(1).SetName("DeleteItems"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Delete(3))
                .Returns(1).SetName("DeleteNonExistingItems"),
        };

        [TestCaseSource(nameof(counterStatTests))]
        public int CounterStat_Should(Event<int>[] events)
        {
            var stat = new CounterStat<int>();
            HandleEvents(stat, events);
            return stat.Value;
        }
    }
}
