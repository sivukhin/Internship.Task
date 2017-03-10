using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public class PlayerStatistics
    {
        public int TotalMatchesPlayed { get; set; }
        public int TotalMatchesWon { get; set; }
        public string FavoriteServer { get; set; }
        public int UniqueServers { get; set; }
        public string FavoriteGameMode { get; set; }
        public double AverageScoreboardPercent { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public DateTime LastMatchPlayed { get; set; }
        public double? KillToDeathRatio { get; set; }
    }

    public interface IPlayerStatisticsProvider
    {
        PlayerStatistics this[string playerName] { get; }
        void Add(PlayerInfo player);
        void Delete(PlayerInfo player);
    }

    public class PlayerStatisticsProvider : BaseStatisticsProvider<PlayerInfo>, IPlayerStatisticsProvider
    {
        private static GroupedStat<PlayerInfo, T, string> CreateStat<T>
            (Func<DataIdentity<PlayerInfo>, IStat<PlayerInfo, T>> statFactory)
        {
            return new GroupedStat<PlayerInfo, T, string>(player => player.Name, () => statFactory(Info));
        }

        public readonly GroupedStat<PlayerInfo, int, string> TotalMatchesPlayed =
            CreateStat(player => player.Count());

        public readonly GroupedStat<PlayerInfo, int, string> TotalMatchesWon =
            CreateStat(player => player.Where(info => info.AreWinner).Count());

        public readonly GroupedStat<PlayerInfo, int, string> TotalKills =
            CreateStat(player => player.Sum(info => info.Kills));

        public readonly GroupedStat<PlayerInfo, int, string> TotalDeaths =
            CreateStat(player => player.Sum(info => info.Deaths));

        public readonly GroupedStat<PlayerInfo, string, string> FavoriteServer =
            CreateStat(player => player.Favorite(info => info.BaseMatch.HostServer.Name));

        public readonly GroupedStat<PlayerInfo, int, string> UniqueServers =
            CreateStat(player => player.Split(info => info.BaseMatch.HostServer.ServerId, splitted => splitted.Existence()).Count());

        public readonly GroupedStat<PlayerInfo, string, string> FavoriteGameMode =
            CreateStat(player => player.Favorite(info => info.BaseMatch.GameMode.ModeName));

        public readonly GroupedStat<PlayerInfo, double, string> AverageScoreboardPercent =
            CreateStat(player => player.Select(info => info.ScoreboardPercent).Average());

        public readonly GroupedStat<PlayerInfo, int, string> MaximumMatchesPerDay =
            CreateStat(player => player.Split(info => info.BaseMatch.EndTime.Date, splitted => splitted.Count()).Max());

        public readonly GroupedStat<PlayerInfo, DateTime, string> FirstMatchPlayed =
            CreateStat(player => player.Min(info => info.BaseMatch.EndTime.Date));

        public readonly GroupedStat<PlayerInfo, DateTime, string> LastMatchPlayed =
            CreateStat(player => player.Max(info => info.BaseMatch.EndTime.Date));

        public double? KillToDeathRatio(string playerName)
        {
            var totalKills = TotalKills[playerName];
            var totalDeaths = TotalDeaths[playerName];
            if (totalDeaths == 0)
                return null;
            return 1.0 * totalKills / totalDeaths;
        }

        public double AverageMatchesPerDay(string playerName)
        {
            return 1.0 * TotalMatchesPlayed[playerName] / ((LastMatchPlayed[playerName] - FirstMatchPlayed[playerName]).Days + 1);
        }

        public PlayerStatistics this[string playerName] => TotalMatchesPlayed[playerName] == 0 ? null : new PlayerStatistics
        {
            MaximumMatchesPerDay = MaximumMatchesPerDay[playerName],
            AverageMatchesPerDay = AverageMatchesPerDay(playerName),
            TotalMatchesPlayed = TotalMatchesPlayed[playerName],
            AverageScoreboardPercent = AverageScoreboardPercent[playerName],
            FavoriteGameMode = FavoriteGameMode[playerName],
            FavoriteServer = FavoriteServer[playerName],
            KillToDeathRatio = KillToDeathRatio(playerName),
            LastMatchPlayed = LastMatchPlayed[playerName],
            TotalMatchesWon = TotalMatchesWon[playerName],
            UniqueServers = UniqueServers[playerName]
        };
    }
}
