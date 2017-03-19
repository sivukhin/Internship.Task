using System;
using System.Collections.Generic;
using System.Linq;
using DataCore;
using NLog;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace Kontur.GameStats.Server.Storage
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

        protected bool Equals(PlayerReportResult other)
        {
            return Equals(Player, other.Player);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerReportResult)obj);
        }

        public override int GetHashCode()
        {
            return Player?.GetHashCode() ?? 0;
        }
    }

    public class ServerReportResult
    {
        public ServerInfo Server { get; set; }
        public double TotalMatchesPlayed { get; set; }

        public double AverageMatchesPerDay(IGlobalServerStatisticStorage globalStatisticStorage)
        {
            return TotalMatchesPlayed / ((globalStatisticStorage.LastDayWithMatch - globalStatisticStorage.FirstDayWithMatch).Days + 1);
        }

        protected bool Equals(ServerReportResult other)
        {
            return Equals(Server, other.Server);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerReportResult)obj);
        }

        public override int GetHashCode()
        {
            return Server?.GetHashCode() ?? 0;
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

        public ReportStorage(
            IServerStatisticStorage serverStatisticStorage, 
            IPlayerStatisticStorage playerStatisticStorage)
        {
            this.serverStatisticStorage = serverStatisticStorage;
            this.playerStatisticStorage = playerStatisticStorage;
            InitReports();
        }

        private void InitReports()
        {
            logger.Info("Initialize reports");

            recentMatches = new DataIdentity<MatchInfo>()
                .Report(MaxReportSize, m => m.EndTime, (m1, m2) => m1.GetIndex().CompareTo(m2.GetIndex()));

            popularServers = new DataIdentity<ServerInfo>()
                .Select(s => new ServerReportResult
                {
                    Server = s,
                    TotalMatchesPlayed = serverStatisticStorage.GetStatistics(s.Id)?.TotalMatchesPlayed ?? 0
                })
                .Report(MaxReportSize,
                    s => s.TotalMatchesPlayed,
                    (s1, s2) => string.Compare(s1.Server.Id, s2.Server.Id, StringComparison.Ordinal));

            allServers = new DataIdentity<ServerInfo>()
                .Report(s => s.Id, (s1, s2) => string.Compare(s1.Id, s2.Id, StringComparison.Ordinal));

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
            (p1, p2) => p1.Player.GetIndex().CompareTo(p2.Player.GetIndex()));
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
