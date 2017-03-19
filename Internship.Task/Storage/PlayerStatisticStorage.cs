using System;
using System.Collections.Generic;
using System.Globalization;
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
    
    public interface IPlayerStatisticStorage
    {
        PlayerStatistics GetStatistics(string playerName);
        void Add(PlayerInfo player);
        void Delete(PlayerInfo player);
    }

    public class PlayerStatisticStorage : BaseStatisticStorage<PlayerInfo>, IPlayerStatisticStorage
    {
        private static GroupedStat<PlayerInfo, T, string> CreateStat<T>
            (Func<DataIdentity<PlayerInfo>, IStat<PlayerInfo, T>> statFactory)
        {
            return new GroupedStat<PlayerInfo, T, string>(player => player.Name.ToLower(CultureInfo.InvariantCulture), () => statFactory(Info));
        }

        private readonly GroupedStat<PlayerInfo, int, string> totalMatchesPlayed =
            CreateStat(player => player.Count());

        private readonly GroupedStat<PlayerInfo, int, string> totalMatchesWon =
            CreateStat(player => player.Where(info => info.AreWinner).Count());

        private readonly GroupedStat<PlayerInfo, int, string> totalKills =
            CreateStat(player => player.Sum(info => info.Kills));

        private readonly GroupedStat<PlayerInfo, int, string> totalDeaths =
            CreateStat(player => player.Sum(info => info.Deaths));

        private readonly GroupedStat<PlayerInfo, string, string> favoriteServer =
            CreateStat(player => player.Select(info => info.BaseMatch.HostServer.Id).Favorite());

        private readonly GroupedStat<PlayerInfo, int, string> uniqueServers =
            CreateStat(player => player.Split(info => info.BaseMatch.HostServer.Id, splitted => splitted.Existence()).Count());

        private readonly GroupedStat<PlayerInfo, string, string> favoriteGameMode =
            CreateStat(player => player.Select(info => info.BaseMatch.GameMode).Favorite());

        private readonly GroupedStat<PlayerInfo, double, string> averageScoreboardPercent =
            CreateStat(player => player.Select(info => info.ScoreboardPercent).Average());

        private readonly GroupedStat<PlayerInfo, int, string> maximumMatchesPerDay =
            CreateStat(player => player.Split(info => info.BaseMatch.EndTime.Date, splitted => splitted.Count()).Max());

        private readonly GroupedStat<PlayerInfo, DateTime, string> firstMatchPlayed =
            CreateStat(player => player.Min(info => info.BaseMatch.EndTime));

        private readonly GroupedStat<PlayerInfo, DateTime, string> lastMatchPlayed =
            CreateStat(player => player.Max(info => info.BaseMatch.EndTime));

        private double? KillToDeathRatio(string playerName)
        {
            var kills = totalKills[playerName];
            var deaths = totalDeaths[playerName];
            if (deaths == 0)
                return null;
            return 1.0 * kills / deaths;
        }

        private double AverageMatchesPerDay(string playerName)
        {
            return 1.0 * totalMatchesPlayed[playerName] / ((lastMatchPlayed[playerName] - firstMatchPlayed[playerName]).Days + 1);
        }

        public PlayerStatistics GetStatistics(string playerName)
        {
            return new PlayerStatistics
            {
                MaximumMatchesPerDay = maximumMatchesPerDay[playerName],
                AverageMatchesPerDay = AverageMatchesPerDay(playerName),
                TotalMatchesPlayed = totalMatchesPlayed[playerName],
                AverageScoreboardPercent = averageScoreboardPercent[playerName],
                FavoriteGameMode = favoriteGameMode[playerName],
                FavoriteServer = favoriteServer[playerName],
                KillToDeathRatio = KillToDeathRatio(playerName),
                LastMatchPlayed = lastMatchPlayed[playerName],
                TotalMatchesWon = totalMatchesWon[playerName],
                UniqueServers = uniqueServers[playerName]
            };
        }
    }
}
