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
    public interface IReportStorage
    {
        void Update(MatchInfo match);
        void Update(ServerInfo server);
        void Update(PlayerInfo player);
        
        IEnumerable<ServerInfo> AllServers();
        IEnumerable<ServerReportResult> PopularServers(int size);
        IEnumerable<PlayerReportResult> BestPlayers(int size);
        IEnumerable<MatchInfo> RecentMatches(int size);
    }
    public class PlayerReportResult
    {
        public PlayerInfo Player { get; set; }
        public double? KillToDeathRatio { get; set; }
    }

    public class ServerReportResult
    {
        public ServerInfo Server { get; set; }
        public double TotalMatchesPlayed { get; set; }

        public double AverageMatchesPerDay(IGlobalServerStatisticStorage globalStatisticStorage)
        {
            return TotalMatchesPlayed / ((globalStatisticStorage.LastDayWithMatch - globalStatisticStorage.FirstDayWithMatch).Days + 1);
        }
    }

    public class ReportStorage : IReportStorage
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private const int MaxReportSize = 50;
        private IStat<MatchInfo, IEnumerable<MatchInfo>> recentMatches;
        private IStat<ServerInfo, IEnumerable<ServerReportResult>> popularServers;
        private IStat<PlayerInfo, IEnumerable<PlayerReportResult>> bestPlayers;
        private IStat<ServerInfo, IEnumerable<ServerInfo>> allServers;
        private readonly IServerStatisticStorage serverStatisticStorage;
        private readonly IPlayerStatisticStorage playerStatisticStorage;
        private readonly IGlobalServerStatisticStorage globalStatisticStorage;

        public ReportStorage(
            IServerStatisticStorage serverStatisticStorage, 
            IPlayerStatisticStorage playerStatisticStorage, 
            IGlobalServerStatisticStorage globalStatisticStorage)
        {
            this.serverStatisticStorage = serverStatisticStorage;
            this.playerStatisticStorage = playerStatisticStorage;
            this.globalStatisticStorage = globalStatisticStorage;
            InitReports();
        }

        private void InitReports()
        {
            logger.Info("Initialize reports");

            recentMatches = new DataIdentity<MatchInfo>()
                .Report(MaxReportSize, m => m.EndTime, (m1, m2) => m1.EndTime.CompareTo(m2.EndTime) < 0);

            popularServers = new DataIdentity<ServerInfo>()
                .Select(s => new ServerReportResult
                {
                    Server = s,
                    TotalMatchesPlayed = serverStatisticStorage.GetStatistics(s.Id)?.TotalMatchesPlayed ?? 0
                })
                .Report(MaxReportSize,
                    s => s.TotalMatchesPlayed,
                    (s1, s2) => string.Compare(s1.Server.Id, s2.Server.Id, StringComparison.Ordinal) < 0);

            allServers = new DataIdentity<ServerInfo>()
                .Report(s => s.Id, (s1, s2) => String.Compare(s1.Id, s2.Id, StringComparison.Ordinal) < 0);

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
            (p1, p2) => String.Compare(p1.Player.Name, p2.Player.Name, StringComparison.Ordinal) < 0);
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
        
        public IEnumerable<ServerReportResult> PopularServers(int size) =>
            popularServers.Value.Take(size).ToList();

        public IEnumerable<ServerInfo> AllServers() =>
            allServers.Value.ToList();

        public IEnumerable<PlayerReportResult> BestPlayers(int size) =>
            bestPlayers.Value.Take(size).ToList();

        public IEnumerable<MatchInfo> RecentMatches(int size) =>
            recentMatches.Value.Take(size).ToList();
    }
}
