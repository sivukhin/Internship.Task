using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using NLog;
using Remotion.Linq.Utilities;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public interface IReportStorage<T>
    {
        void Update(T item);
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
        private Logger logger = LogManager.GetCurrentClassLogger();

        private const int MaxReportSize = 50;
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
            logger.Info("Initialize reports");

            recentMatches = new DataIdentity<MatchInfo>().Report(MaxReportSize, m => m.EndTime, (m1, m2) => m1.MatchId < m2.MatchId);
            popularServers = new DataIdentity<ServerInfo>().Report(MaxReportSize,
                s => serverStatisticStorage.GetStatistics(s.Name).AverageMatchesPerDay,
                (s1, s2) => string.Compare(s1.ServerId, s2.ServerId, StringComparison.Ordinal) == -1);

            bestPlayers = new DataIdentity<PlayerInfo>().Where(p =>
            {
                var playerStat = playerStatisticStorage.GetStatistics(p.Name);
                return playerStat.KillToDeathRatio != null && playerStat.TotalMatchesPlayed >= 10;
            })
            .Report(MaxReportSize, p =>
            {
                var killToDeathRatio = playerStatisticStorage.GetStatistics(p.Name).KillToDeathRatio;
                if (killToDeathRatio != null)
                    return killToDeathRatio.Value;
                throw new ArgumentEmptyException($"{nameof(killToDeathRatio)} must be not null");
            },
            (p1, p2) => p1.PlayerId < p2.PlayerId);
        }

        public void Update(ServerInfo serverInfo)
        {
            logger.Info("Update reports with server: {0}", serverInfo);
            popularServers.Add(serverInfo);
        }

        public void Update(PlayerInfo playerInfo)
        {
            logger.Info("Update reports with player: {0}", playerInfo);
            bestPlayers.Add(playerInfo);
        }

        public void Update(MatchInfo matchInfo)
        {
            logger.Info("Update reports with match: {0}", matchInfo);
            recentMatches.Add(matchInfo);
        }
        
        IEnumerable<ServerInfo> IReportStorage<ServerInfo>.Report(int size) =>
            popularServers.Value.Take(size).ToList();
        
        IEnumerable<PlayerInfo> IReportStorage<PlayerInfo>.Report(int size) =>
            bestPlayers.Value.Take(size).ToList();

        IEnumerable<MatchInfo> IReportStorage<MatchInfo>.Report(int size) =>
            recentMatches.Value.Take(size).ToList();
    }
}
