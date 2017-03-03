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
    class GroupedAverageStatTests : BaseStatTest
    {
        private IStat<int, double> stat;

        [SetUp]
        public void SetUp()
        {
            stat = new GroupedAverageStat<int, int>(item => item, () => StatFor<int>.Count().Select(i => (double)i));
        }

        [Test]
        public void GroupedStat_ReturnZero_WhenNoItemsAdded()
        {
            stat.Value.Should().Be(0);
        }

        private static readonly TestCaseData[] groupedAverageStatTests = {
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1))
                .Returns(1).SetName("AllItemsDistinct"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(2), Event<int>.Add(2))
                .Returns(3).SetName("AllItemsEqual"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Add(2))
                .Returns(1.5).SetName("DistinctAndEqualItems"),
            ParamsTestCaseData.Create(Event<int>.Add(2), Event<int>.Add(1), Event<int>.Add(2), Event<int>.Delete(2))
                .Returns(1).SetName("DeleteItems"),
        };

        [TestCaseSource(nameof(groupedAverageStatTests))]
        public double GroupedStat_ReturnsAverageAmountOfItems_WithEqualValue(Event<int>[] items)
        {
            HandleEvents(stat, items);
            return stat.Value;
        }
    }
}
