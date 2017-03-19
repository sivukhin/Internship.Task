using System;
using System.Threading.Tasks;
using DataCore;
using FluentAssertions;
using Kontur.GameStats.Server.Modules;
using NUnit.Framework;

namespace Kontur.GameStats.Server.Tests.Modules.StatisticModuleTests
{
    class PlayerStatisticTests : BaseModuleTests
    {
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
            var matches = new[]
            {
                GenerateMatch(Server1, DateTime1, scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, DateTime2, scoreboard: new[] {Player2})
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).TotalMatchesPlayed.Should().Be(1);
            PlayerStatisticStorage.GetStatistics(Player2.Name).TotalMatchesPlayed.Should().Be(2);
        }

        [Test]
        public async Task TotalMatchesWonTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            var matches = new[]
            {
                GenerateMatch(Server1, Day1, scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, Day2, scoreboard: new[] {Player1}),
                GenerateMatch(Server1, Day3, scoreboard: new[] {Player2, Player1}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).TotalMatchesWon.Should().Be(2);
            PlayerStatisticStorage.GetStatistics(Player2.Name).TotalMatchesWon.Should().Be(1);
        }

        [Test]
        public async Task FavoriteServerTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, Day1, scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server2, Day2, scoreboard: new[] {Player1}),
                GenerateMatch(Server1, Day3, scoreboard: new[] {Player2, Player1}),
                GenerateMatch(Server1, Day4, scoreboard: new[] {Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).FavoriteServer.Should().Be(Server2.Id);
            PlayerStatisticStorage.GetStatistics(Player2.Name).FavoriteServer.Should().Be(Server1.Id);
        }

        [Test]
        public async Task UniqueServersTest()
        {
            var server3 = GenerateServer("host-3");
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            await StatisticStorage.UpdateServer(server3.GetIndex(), server3);
            var matches = new[]
            {
                GenerateMatch(Server2, Day1, scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, Day2, scoreboard: new[] {Player1}),
                GenerateMatch(Server2, Day3, scoreboard: new[] {Player2, Player1}),
                GenerateMatch(server3, Day4, scoreboard: new[] {Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).UniqueServers.Should().Be(2);
            PlayerStatisticStorage.GetStatistics(Player2.Name).UniqueServers.Should().Be(2);
        }

        [Test]
        public async Task FavoriteGameModeTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, Day1, scoreboard: new[] {Player1, Player2}, gameMode:"A"),
                GenerateMatch(Server1, Day2, scoreboard: new[] {Player1}, gameMode:"B"),
                GenerateMatch(Server2, Day3, scoreboard: new[] {Player2, Player1}, gameMode:"B"),
                GenerateMatch(Server2, Day4, scoreboard: new[] {Player2}, gameMode:"A"),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).FavoriteGameMode.Should().Be("B");
            PlayerStatisticStorage.GetStatistics(Player2.Name).FavoriteGameMode.Should().Be("A");
        }

        [Test]
        public async Task AverageScoreboardPercentTest()
        {
            PlayerInfo
                player1 = GeneratePlayer("A"),
                player2 = GeneratePlayer("B"),
                player3 = GeneratePlayer("C"),
                player4 = GeneratePlayer("D");
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, Day1, scoreboard: new[] {player1, player2, player4}),
                GenerateMatch(Server1, Day2, scoreboard: new[] {player3, player2, player1, player4}),
                GenerateMatch(Server2, Day3, scoreboard: new[] {player2, player4, player1}),
                GenerateMatch(Server2, Day4, scoreboard: new[] {player3, player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(player1.Name).AverageScoreboardPercent
                .Should().Be((100.0 + 100.0 / 3 + 0) / 3);
            PlayerStatisticStorage.GetStatistics(player2.Name).AverageScoreboardPercent
                .Should().Be((100.0 / 2 + 100.0 * 2 / 3 + 100.0 + 0) / 4);
            PlayerStatisticStorage.GetStatistics(player3.Name).AverageScoreboardPercent
                .Should().Be((100.0 + 100.0) / 2);
            PlayerStatisticStorage.GetStatistics(player4.Name).AverageScoreboardPercent
                .Should().Be((0.0 + 0.0 + 100.0 / 2) / 3);
        }

        [Test]
        public async Task MaximumMatchesPerDayTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, new DateTime(2017, 1, 1, 12, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, new DateTime(2017, 1, 1, 13, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1}),
                GenerateMatch(Server2, new DateTime(2017, 1, 2, 14, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player2, Player1}),
                GenerateMatch(Server2, new DateTime(2017, 1, 1, 15, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1, Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).MaximumMatchesPerDay.Should().Be(3);
            PlayerStatisticStorage.GetStatistics(Player2.Name).MaximumMatchesPerDay.Should().Be(2);
        }
            
        [Test]
        public async Task AverageMatchesPerDayTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, new DateTime(2017, 1, 1, 12, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, new DateTime(2017, 1, 8, 13, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1}),
                GenerateMatch(Server2, new DateTime(2017, 1, 2, 14, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player2, Player1}),
                GenerateMatch(Server2, new DateTime(2017, 1, 1, 15, 00, 00, DateTimeKind.Utc), scoreboard: new[] {Player1, Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).AverageMatchesPerDay.Should().Be(0.5);
            PlayerStatisticStorage.GetStatistics(Player2.Name).AverageMatchesPerDay.Should().Be(1.5);
        }

        [Test]
        public async Task LastMatchPlayedTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);
            var matches = new[]
            {
                GenerateMatch(Server2, Day1, scoreboard: new[] {Player1, Player2}),
                GenerateMatch(Server1, Day2, scoreboard: new[] {Player1}),
                GenerateMatch(Server2, Day3, scoreboard: new[] {Player1}),
                GenerateMatch(Server2, Day4, scoreboard: new[] {Player2}),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics(Player1.Name).LastMatchPlayed.Should().Be(Day3);
            PlayerStatisticStorage.GetStatistics(Player2.Name).LastMatchPlayed.Should().Be(Day4);
        }

        [Test]
        public async Task KillToDeathRatioTest()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            var matches = new[]
            {
                GenerateMatch(Server1, Day1, scoreboard: new[]
                {
                    GeneratePlayer("A", kills:2, deaths:1), GeneratePlayer("B", kills:3, deaths:0)
                }),
                GenerateMatch(Server1, Day2, scoreboard: new[]
                {
                    GeneratePlayer("A", kills:4, deaths:3)
                }),
            };
            foreach (var match in matches)
                await StatisticStorage.UpdateMatch(match.GetIndex(), match);
            await WaitForTasks();

            PlayerStatisticStorage.GetStatistics("A").KillToDeathRatio.Should().Be(1.5);
            PlayerStatisticStorage.GetStatistics("B").KillToDeathRatio.Should().Be(null);
        }
    }
}
