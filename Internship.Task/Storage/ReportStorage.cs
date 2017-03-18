using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using NLog;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public class PlayerReportResult
    {
        public PlayerInfo Player { get; set; }
        public double? KillToDeathRatio { get; set; }
    }
    public class ReportStorage
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private const int MaxReportSize = 50;
        private IStat<MatchInfo, IEnumerable<MatchInfo>> recentMatches;
        private IStat<ServerInfo, IEnumerable<ServerInfo>> popularServers;
        private IStat<PlayerInfo, IEnumerable<PlayerReportResult>> bestPlayers;
        private IStat<ServerInfo, IEnumerable<ServerInfo>> allServers;
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

            recentMatches = new DataIdentity<MatchInfo>().Report(MaxReportSize, m => m.EndTime, (m1, m2) => m1.EndTime < m2.EndTime);
            popularServers = new DataIdentity<ServerInfo>().Report(MaxReportSize,
                s => serverStatisticStorage.GetStatistics(s.Name).AverageMatchesPerDay,
                (s1, s2) => string.Compare(s1.Id, s2.Id, StringComparison.Ordinal) == -1);
            allServers = new DataIdentity<ServerInfo>().Report(s => s.Id, (s1, s2) => String.Compare(s1.Id, s2.Id, StringComparison.Ordinal) == -1);

            bestPlayers = new DataIdentity<PlayerInfo>().Where(p =>
            {
                var playerStat = playerStatisticStorage.GetStatistics(p.Name);
                return playerStat.KillToDeathRatio != null && playerStat.TotalMatchesPlayed >= 10;
            })
            .Select(p => new PlayerReportResult { Player = p, KillToDeathRatio = playerStatisticStorage.GetStatistics(p.Name).KillToDeathRatio})
            .Report(MaxReportSize, p =>
            {
                if (p.KillToDeathRatio != null)
                    return p.KillToDeathRatio.Value;
                throw new ArgumentException($"{nameof(p.KillToDeathRatio)} must be not null");
            },
            (p1, p2) => String.Compare(p1.Player.Name, p2.Player.Name, StringComparison.Ordinal) == -1);
        }

        public void Update(ServerInfo serverInfo)
        {
            logger.ConditionalTrace("Update reports with server: {0}", serverInfo);
            allServers.Add(serverInfo);
            popularServers.Add(serverInfo);
        }

        public void Update(PlayerInfo playerInfo)
        {
            logger.ConditionalTrace("Update reports with player: {0}", playerInfo);
            bestPlayers.Add(playerInfo);
        }

        public void Update(MatchInfo matchInfo)
        {
            logger.ConditionalTrace("Update reports with match: {0}", matchInfo);
            recentMatches.Add(matchInfo);
        }
        
        public IEnumerable<ServerInfo> PopularServers(int size) =>
            popularServers.Value.Take(size).ToList();

        public IEnumerable<ServerInfo> AllServers() =>
            allServers.Value.ToList();

        public IEnumerable<PlayerReportResult> BestPlayers(int size) =>
            bestPlayers.Value.Take(size).ToList();

        public IEnumerable<MatchInfo> RecentMatches(int size) =>
            recentMatches.Value.Take(size).ToList();
    }
}
