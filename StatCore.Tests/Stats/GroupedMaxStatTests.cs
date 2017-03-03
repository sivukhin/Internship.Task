using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using StatCore.Stats;

namespace StatCore.Tests.Stats
{
    [TestFixture]
    class GroupedMaxStatTests : BaseStatTest
    {
        private IStat<int, int> stat;
        [SetUp]
        public void SetUp()
        {
            stat = new GroupedMaxStat<int, int, int>(i => i, () => StatFor<int>.Count());
        }

        [Test]
        public void GroupedStat_ReturnZero_WhenNoItemsAdded()
        {
            stat.Value.Should().Be(0);
        }

        private static readonly TestCaseData[] groupedMaxStatTests = {
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1))
                .Returns(1).SetName("AllItemsDistinct"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(2), Event<int>.Add(2))
                .Returns(3).SetName("AllItemsEqual"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Add(2))
                .Returns(2).SetName("DistinctAndEqualItems"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Add(2), Event<int>.Delete(2))
                .Returns(1).SetName("DeleteItems"),
        };

        [TestCaseSource(nameof(groupedMaxStatTests))]
        public int GroupedStat_ReturnsMaximumAmountOfItems_WithEqualValue(Event<int>[] events)
        {
            HandleEvents(stat, events);
            return stat.Value;
        }
    }
}
