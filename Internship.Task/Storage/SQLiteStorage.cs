using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using NHibernate;
using NLog;

namespace StatisticServer.Storage
{
    public class SQLiteStorage : IStatisticStorage
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Task EmptyTask => Task.FromResult(0);
        private readonly ISessionFactory sessionsFactory;
        private readonly IServerStatisticStorage serverStatisticStorage;
        private readonly IPlayerStatisticStorage playerStatisticStorage;
        private readonly IAggregateReportStorage reportStorage;

        public SQLiteStorage(
            ISessionFactory sessionsFactory, 
            IServerStatisticStorage serverStatisticStorage, 
            IPlayerStatisticStorage playerStatisticStorage, 
            IAggregateReportStorage reportStorage)
        {
            logger.Info("Initialize SQLite storage");

            this.sessionsFactory = sessionsFactory;
            this.serverStatisticStorage = serverStatisticStorage;
            this.playerStatisticStorage = playerStatisticStorage;
            this.reportStorage = reportStorage;
            InitStatisticsProviders();
        }

        private void InitStatisticsProviders()
        {
            InitServerStatisticsProvider();
        }

        private void InitServerStatisticsProvider()
        {
            logger.Info("Initialize server statistics from database");
            using (var session = sessionsFactory.OpenSession())
            {
                var matches = session.QueryOver<MatchInfo>().List();
                int processedMatches = 0, processedPlayers = 0;
                foreach (var match in matches)
                {
                    serverStatisticStorage.Add(match);
                    processedMatches++;
                    foreach (var player in match.Scoreboard)
                    {
                        playerStatisticStorage.Add(player);
                        processedPlayers++;
                    }
                }
                logger.Info($"Successfully processed {processedMatches} match entries");
                logger.Info($"Successfully processed {processedPlayers} players entries");
            }
        }

        public Task UpdateServerInfo(string serverId, ServerInfo info)
        {
            info.ServerId = serverId;
            logger.Trace("Update information about server {0}", info);

            using (var session = sessionsFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(info);
                transaction.Commit();
                return EmptyTask;
            }
        }

        public Task<ServerInfo> GetServerInfo(string serverId)
        {
            logger.Trace("Retrive information about server {0}", new {ServerId = serverId});

            using (var session = sessionsFactory.OpenSession())
            using (session.BeginTransaction())
            {
                var server = session.Get<ServerInfo>(serverId);
                return Task.FromResult(server);
            }
        }

        public Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            logger.Trace("Retrieve information about all servers");

            using (var session = sessionsFactory.OpenSession())
            using (session.BeginTransaction())
            {
                return Task.FromResult(session.CreateCriteria<ServerInfo>().List<ServerInfo>().AsEnumerable());
            }
        }

        public Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            logger.Trace("Update information about match {0}",
                new {ServerId = serverId, EndTime = endTime, MatchInfo = matchInfo});

            using (var session = sessionsFactory.OpenSession())
            {
                var hostServer = session.Get<ServerInfo>(serverId);
                if (hostServer == null)
                    throw new ArgumentException($"Unknown serverId: {serverId}", nameof(serverId));

                matchInfo.HostServer = hostServer;
                matchInfo.EndTime = endTime;
                var oldMatchInfo = session.Get<MatchInfo>(matchInfo.MatchId);
                if (oldMatchInfo != null)
                    DeleteMatch(oldMatchInfo);
                InsertMatch(matchInfo);
                session.SaveOrUpdate(matchInfo);
            }
            return EmptyTask;
        }

        private void DeleteMatch(MatchInfo matchInfo)
        {
            serverStatisticStorage.Delete(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Delete(player);
            Task.Factory.StartNew(() => reportStorage.Update(matchInfo))
                .ContinueWith(_ => reportStorage.Update(matchInfo.HostServer))
                .ContinueWith(_ =>
                {
                    foreach (var player in matchInfo.Scoreboard)
                        reportStorage.Update(player);
                });
        }

        private void InsertMatch(MatchInfo matchInfo)
        {
            serverStatisticStorage.Add(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Add(player);
            Task.Factory.StartNew(() => reportStorage.Update(matchInfo))
                .ContinueWith(_ => reportStorage.Update(matchInfo.HostServer))
                .ContinueWith(_ =>
                {
                    foreach (var player in matchInfo.Scoreboard)
                        reportStorage.Update(player);
                });
        }

        public Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
            logger.Trace("Retrieve information about match {0}", new {ServerId = serverId, EndTime = endTime});

            using (var session = sessionsFactory.OpenSession())
            {
                var hostServer = session.Get<ServerInfo>(serverId);
                if (hostServer == null)
                    return Task.FromResult<MatchInfo>(null);
                var matchInfo = session.QueryOver<MatchInfo>().Where(info => info.EndTime == endTime).SingleOrDefault();
                if (matchInfo == null)
                    return Task.FromResult<MatchInfo>(null);
                return Task.FromResult(matchInfo);
            }
        }
    }
}
