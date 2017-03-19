using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FluentAssertions;
using NUnit.Framework;
using StatisticServer.Modules;

namespace StatisticServer.Tests.Modules.StatisticModuleTests
{
    [TestFixture]
    class ServerStatisticTests : BaseModuleTests
    {
        public static DateTime Day1 = new DateTime(2017, 2, 1, 12, 00, 00);
        public static DateTime Day2 = new DateTime(2017, 2, 2, 12, 00, 00);
        public static DateTime Day3 = new DateTime(2017, 2, 3, 12, 00, 00);
        public static DateTime Day4 = new DateTime(2017, 2, 4, 12, 00, 00);
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Module = new StatisticModule(ServerStatisticStorage, PlayerStatisticStorage);
        }

        [Test]
        public async Task TotalMatchesPlayedTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            MatchInfo match1 = GenerateMatch(Server1, Day1), match2 = GenerateMatch(Server1, Day3);
            await StatisticStorage.UpdateMatch(match1.GetIndex(), match1);
            await StatisticStorage.UpdateMatch(match2.GetIndex(), match2);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).TotalMatchesPlayed.Should().Be(2);
        }

        [Test]
        public async Task MaximumMatchesPerDayTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            MatchInfo[] matches = {
                GenerateMatch(Server1, new DateTime(2017, 2, 1, 12, 00, 00)),
                GenerateMatch(Server1, new DateTime(2017, 2, 1, 13, 00, 00)),
                GenerateMatch(Server1, new DateTime(2017, 2, 1, 00, 00, 00)),
                GenerateMatch(Server1, new DateTime(2017, 2, 2, 00, 00, 00)),
                GenerateMatch(Server1, new DateTime(2017, 2, 2, 00, 00, 00)),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).MaximumMatchesPerDay.Should().Be(3);
        }

        [Test]
        public async Task AverageMatchesPerDayTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            MatchInfo[] matches = {
                GenerateMatch(Server1, Day2),
                GenerateMatch(Server1, Day3),
                GenerateMatch(Server2, Day1),
                GenerateMatch(Server2, Day4),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).AverageMatchesPerDay.Should().Be(0.5);
        }

        [Test]
        public async Task MaximumPopulationTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            MatchInfo[] matches = {
                GenerateMatch(Server1, Day2, "map", new [] {Player1, Player2}),
                GenerateMatch(Server1, Day3, "map", new [] {Player1, Player2, GeneratePlayer("A")}),
                GenerateMatch(Server2, Day1, "map", new [] {Player1, Player2, GeneratePlayer("A"), GeneratePlayer("B")}),
                GenerateMatch(Server2, Day4),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).MaximumPopulation.Should().Be(3);
            ServerStatisticStorage.GetStatistics(Server2.Id).MaximumPopulation.Should().Be(4);
        }

        [Test]
        public async Task AveragePopulationTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            MatchInfo[] matches = {
                GenerateMatch(Server1, Day2, "map", new [] {Player1, Player2}),
                GenerateMatch(Server1, Day3, "map", new [] {Player1, Player2, GeneratePlayer("A")}),
                GenerateMatch(Server2, Day1, "map", new [] {Player1, Player2, GeneratePlayer("A"), GeneratePlayer("B")}),
                GenerateMatch(Server2, Day4, "map", new[] {Player1, Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).AveragePopulation.Should().Be(2.5);
            ServerStatisticStorage.GetStatistics(Server2.Id).AveragePopulation.Should().Be(3);
        }


        [Test]
        public async Task Top5GameModesTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            MatchInfo[] matches = {
                GenerateMatch(Server1, new DateTime(2017, 1, 1), gameMode:"A"),
                GenerateMatch(Server1, new DateTime(2017, 1, 2), gameMode:"B"),
                GenerateMatch(Server1, new DateTime(2017, 1, 3), gameMode:"C"),
                GenerateMatch(Server1, new DateTime(2017, 1, 4), gameMode:"C"),
                GenerateMatch(Server1, new DateTime(2017, 1, 5), gameMode:"B"),
                GenerateMatch(Server1, new DateTime(2017, 1, 6), gameMode:"A"),
                GenerateMatch(Server1, new DateTime(2017, 1, 7), gameMode:"D"),
                GenerateMatch(Server1, new DateTime(2017, 1, 8), gameMode:"D"),
                GenerateMatch(Server1, new DateTime(2017, 1, 9), gameMode:"E"),
                GenerateMatch(Server1, new DateTime(2017, 1, 10), gameMode:"F"),
                GenerateMatch(Server1, new DateTime(2017, 1, 11), gameMode:"F"),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).Top5GameModes.ShouldBeEquivalentTo(new[] {"A", "B", "C", "D", "F"});
        }

        [Test]
        public async Task Top5MapsTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            MatchInfo[] matches = {
                GenerateMatch(Server1, new DateTime(2017, 1, 1), "A"),
                GenerateMatch(Server1, new DateTime(2017, 1, 2), "B"),
                GenerateMatch(Server1, new DateTime(2017, 1, 3), "C"),
                GenerateMatch(Server1, new DateTime(2017, 1, 4), "C"),
                GenerateMatch(Server1, new DateTime(2017, 1, 5), "B"),
                GenerateMatch(Server1, new DateTime(2017, 1, 6), "A"),
                GenerateMatch(Server1, new DateTime(2017, 1, 7), "D"),
                GenerateMatch(Server1, new DateTime(2017, 1, 8), "D"),
                GenerateMatch(Server1, new DateTime(2017, 1, 9), "E"),
                GenerateMatch(Server1, new DateTime(2017, 1, 10), "F"),
                GenerateMatch(Server1, new DateTime(2017, 1, 11), "F"),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            ServerStatisticStorage.GetStatistics(Server1.Id).Top5Maps.ShouldBeEquivalentTo(new[] { "A", "B", "C", "D", "F" });
        }
    }
}
