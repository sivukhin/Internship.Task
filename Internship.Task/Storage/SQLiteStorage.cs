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
        private ISessionFactory sessionsFactory;
        private IServerStatisticProvider serverStatisticProvider;

        public SQLiteStorage(ISessionFactory sessionsFactory)
        {
            logger.Info("Initialize SQLite storage");

            this.sessionsFactory = sessionsFactory;
            serverStatisticProvider = new ServerStatisticProvider();
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
                foreach (var match in matches)
                {
                    serverStatisticProvider.Add(match);
                }
                logger.Info($"Successfully processed {matches.Count} match entries");
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
                using (var transaction = session.BeginTransaction())
                {
                    var oldMatchInfo = session.Get<MatchInfo>(matchInfo.MatchId);
                    if (oldMatchInfo != null)
                        serverStatisticProvider.Delete(oldMatchInfo);

                    session.SaveOrUpdate(matchInfo);
                    serverStatisticProvider.Add(matchInfo);
                    transaction.Commit();
                }
            }
            return EmptyTask;
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

        public Task<ServerStatistic> GetServerStatistics(string serverId)
        {
            logger.Trace("Retrieve statistics for server {0}", new {ServerId = serverId});

            return Task.FromResult(serverStatisticProvider[serverId]);
        }
    }
}
