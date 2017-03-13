using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public interface IReportStorage<T>
    {
        void Add(T item);
        void Delete(T item);
        IEnumerable<T> Report(int size);
    }

    public interface IAggregateReportStorage : 
        IReportStorage<ServerInfo>, 
        IReportStorage<MatchInfo>,
        IReportStorage<PlayerInfo>
    {
        
    }
    public class ReportStorage : IAggregateReportStorage
    {
        public const int MaxReportSize = 50;
        private IStat<MatchInfo, IEnumerable<MatchInfo>> recentMatches;
        private IStat<ServerInfo, IEnumerable<ServerInfo>> popularServers;
        private IStat<PlayerInfo, IEnumerable<PlayerInfo>> bestPlayers;
        private readonly IServerStatisticStorage serverStatisticStorage;
        private readonly IPlayerStatisticStorage playerStatisticStorage;

        public ReportStorage(IServerStatisticStorage serverStatisticStorage, IPlayerStatisticStorage playerStatisticStorage)
        {
            this.serverStatisticStorage = serverStatisticStorage;
            this.playerStatisticStorage = playerStatisticStorage;
            InitReports();
        }

        private void InitReports()
        {
            recentMatches = new DataIdentity<MatchInfo>().Report(MaxReportSize, (m1, m2) => m1.EndTime > m2.EndTime);
            popularServers = new DataIdentity<ServerInfo>().Report(MaxReportSize, (s1, s2) =>
            {
                var firstPopularity = serverStatisticStorage.GetStatistics(s1.Name).AverageMatchesPerDay;
                var secondPopularity = serverStatisticStorage.GetStatistics(s2.Name).AverageMatchesPerDay;
                return firstPopularity > secondPopularity;
            });
            bestPlayers = new DataIdentity<PlayerInfo>().Where(p =>
            {
                var playerStat = playerStatisticStorage.GetStatistics(p.Name);
                return playerStat.KillToDeathRatio != null && playerStat.TotalMatchesPlayed >= 10;
            }).Report(MaxReportSize, (p1, p2) =>
            {
                var firstPlayerStat = playerStatisticStorage.GetStatistics(p1.Name);
                var secondPlayerStat = playerStatisticStorage.GetStatistics(p2.Name);
                return firstPlayerStat.KillToDeathRatio > secondPlayerStat.KillToDeathRatio;
            });
        }

        public void Add(ServerInfo serverInfo)
        {
            popularServers.Add(serverInfo);
        }

        public void Add(PlayerInfo playerInfo)
        {
            bestPlayers.Add(playerInfo);
        }

        public void Add(MatchInfo matchInfo)
        {
            recentMatches.Add(matchInfo);
        }

        public void Delete(ServerInfo serverInfo)
        {
            popularServers.Delete(serverInfo);
        }

        IEnumerable<ServerInfo> IReportStorage<ServerInfo>.Report(int size) =>
            popularServers.Value.Take(size);

        public void Delete(PlayerInfo playerInfo)
        {
            bestPlayers.Delete(playerInfo);
        }

        IEnumerable<PlayerInfo> IReportStorage<PlayerInfo>.Report(int size) =>
            bestPlayers.Value.Take(size);

        public void Delete(MatchInfo matchInfo)
        {
            recentMatches.Delete(matchInfo);
        }

        IEnumerable<MatchInfo> IReportStorage<MatchInfo>.Report(int size) =>
            recentMatches.Value.Take(size);
    }
}
