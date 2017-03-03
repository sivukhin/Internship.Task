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
    class PopularStatTests : BaseStatTest
    {
        private IStat<string, IEnumerable<string>> stat;

        [SetUp]
        public  void SetUp()
        {
            stat = new PopularStat<string, string>(3, s => s);
        }

        [Test]
        public void PopularStat_ReturnNothing_WhenNoItemsAdded()
        {
            stat.Value.Should().BeEquivalentTo();
        }

        [Test]
        public void PopularStat_ReturnElements_SortedByOccurences()
        {
            HandleEvents(stat, Event<string>.Add("A"), Event<string>.Add("B"), Event<string>.Add("B"));
            stat.Value.ShouldBeEquivalentTo(new[] {"B", "A"}, options => options.WithStrictOrdering());
        }

        [Test]
        public void PopularStat_ReturnElements_SortedByOccurences_WhenDeleteElements()
        {
            HandleEvents(stat, 
                Event<string>.Add("A"), Event<string>.Add("B"), Event<string>.Add("B"), Event<string>.Add("A"), Event<string>.Add("A"),
                Event<string>.Delete("A"), Event<string>.Delete("A"));
            stat.Value.ShouldBeEquivalentTo(new[] { "B", "A" }, options => options.WithStrictOrdering());
        }

        [Test]
        public void PopularStat_ReturnNoMoreThan_MaxSize_Items()
        {
            HandleEvents(stat, Event<string>.Add("A"), Event<string>.Add("B"), Event<string>.Add("C"), Event<string>.Add("D"));
            stat.Value.OrderBy(s => s).ToList()
                .Should().Match<List<string>>(seq =>
                seq.SequenceEqual(new[] {"A", "B", "C"}) ||
                seq.SequenceEqual(new[] { "A", "B", "D" }) ||
                seq.SequenceEqual(new[] { "B", "C", "D" })
            );
        }

        [Test]
        public void PopularStat_WorksWithDeleting()
        {
            HandleEvents(stat, 
                Event<string>.Add("A"), Event<string>.Add("B"), Event<string>.Add("C"), Event<string>.Add("D"), 
                Event<string>.Delete("B"));
            stat.Value.ShouldBeEquivalentTo(new[] { "A", "D", "C" });
        }

        [Test]
        public void PopularStat_ReturnNothing_WhenAllItemsDeleted()
        {
            HandleEvents(stat,
                Event<string>.Add("A"), Event<string>.Add("B"),
                Event<string>.Delete("A"),
                Event<string>.Add("C"),
                Event<string>.Delete("B"), Event<string>.Delete("C"));
            stat.Value.Should().BeEquivalentTo();
        }

        [Test]
        public void PopularStat_ShouldNotDelete_NonExistingItems()
        {
            HandleEvents(stat,
                Event<string>.Add("A"), Event<string>.Delete("B"));
            stat.Value.Should().BeEquivalentTo("A");
        }
    }
}
