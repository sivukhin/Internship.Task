using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using StatCore.Stats;

namespace StatCore.Tests.Stats
{
    [TestFixture]
    class AverageStatTests : BaseStatTest
    {
        private AverageValue HandleEvents(IEnumerable<Event<double>> events)
        {
            var averageValue = new AverageValue();
            foreach (var e in events)
            {
                if (e.IsAddEvent)
                    averageValue += e.Value;
                else
                    averageValue -= e.Value;
            }
            return averageValue;
        }

        private static readonly TestCaseData[] averageTestCases =
        {
            ParamsTestCaseData.Create<Event<double>>()
                .Returns(0).SetName("ReturnZero_WhenNoItemsAdded"),
            ParamsTestCaseData.Create(Event<double>.Add(1))
                .Returns(1).SetName("CalculateAverageValue"),
            ParamsTestCaseData.Create(Event<double>.Add(1), Event<double>.Add(2))
                .Returns(1.5).SetName("CalculateAverageValue"),
            ParamsTestCaseData.Create(Event<double>.Add(1), Event<double>.Add(2), Event<double>.Delete(1))
                .Returns(2).SetName("CalculateAverageValue"),
            ParamsTestCaseData.Create(Event<double>.Add(2), Event<double>.Add(2), Event<double>.Delete(-2))
                .Returns(6).SetName("DeleteNonExistingItems"),
            ParamsTestCaseData.Create(Event<double>.Delete(1))
                .Returns(0).SetName("NotDeleteWhenNoItemsAdded"),
            ParamsTestCaseData.Create(Event<double>.Add(2), Event<double>.Delete(1), Event<double>.Add(2))
                .Returns(3).SetName("WorkWithInvalidInvariants")
        };

        [TestCaseSource(nameof(averageTestCases))]
        public double AverageValue_Should(params Event<double>[] events)
        {
            var average = HandleEvents(events);
            return average.Value;
        }

        [TestCaseSource(nameof(averageTestCases))]
        public double AverageStat_Should(params Event<double>[] events)
        {
            var stat = new AverageStat<double>(d => d);
            HandleEvents(stat, events);
            return stat.Value;
        }
    }
}
