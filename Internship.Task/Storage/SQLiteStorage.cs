using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using NHibernate;

namespace StatisticServer.Storage
{
    public class SQLiteStorage : IStatisticStorage
    {
        private static Task EmptyTask => Task.FromResult(0);
        private ISessionFactory sessionsFactory;
        public SQLiteStorage(ISessionFactory sessionsFactory)
        {
            this.sessionsFactory = sessionsFactory;
        }

        public Task UpdateServerInfo(string serverId, ServerInfo info)
        {
            info.ServerId = serverId;
            using (var session = sessionsFactory.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(info);
                transaction.Commit();
            }
            return EmptyTask;
        }

        public Task<ServerInfo> GetServerInfo(string serverId)
        {
            using (var session = sessionsFactory.OpenSession())
            using (session.BeginTransaction())
            {
                var server = session.Get<ServerInfo>(serverId);
                return Task.FromResult(server);
            }
        }

        public Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            using (var session = sessionsFactory.OpenSession())
            using (session.BeginTransaction())
            {
                return Task.FromResult(session.CreateCriteria<ServerInfo>().List<ServerInfo>().AsEnumerable());
            }
        }

        public Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            using (var session = sessionsFactory.OpenSession())
            {
                var hostServer = session.Get<ServerInfo>(serverId);
                if (hostServer == null)
                    throw new ArgumentException($"Unknown serverId: {serverId}", nameof(serverId));

                matchInfo.HostServer = hostServer;
                matchInfo.EndTime = endTime;
                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(matchInfo);
                    transaction.Commit();
                }
            }
            return EmptyTask;
        }

        public Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
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
