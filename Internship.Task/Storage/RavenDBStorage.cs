using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatabaseCore;
using DataCore;
using Raven.Client;

namespace StatisticServer.Storage
{
    public class RavenDbStorage : IStatisticStorage, IDisposable
    {
        private readonly IDocumentStore store;

        public RavenDbStorage(IDocumentStore store)
        {
            this.store = store;
        }

        public async Task UpdateServerInfo(string serverId, ServerInfo info)
        {
            using (var session = store.OpenAsyncSession())
            {
                await session.StoreAsync(info);
                await session.SaveChangesAsync();
            }
        }

        public async Task<ServerInfo> GetServerInfo(string serverId)
        {
            using (var session = store.OpenAsyncSession())
            {
                return await session.Query<ServerInfo, Server_ById>().Where(s => s.Id == serverId).SingleOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            var result = new List<ServerInfo>();
            using (var session = store.OpenAsyncSession())
            {
                var pageSize = 1024;
                RavenQueryStatistics queryStat;
                var firstPage = await session.Query<ServerInfo>().Statistics(out queryStat).Take(pageSize).ToListAsync();
                var remain = queryStat.TotalResults - firstPage.Count;
                result.AddRange(firstPage);
                var processed = firstPage.Count;
                while (remain > 0)
                {
                    var nextPage = await session.Query<ServerInfo>().Skip(processed).Take(pageSize).ToListAsync();
                    remain -= nextPage.Count;
                    processed += nextPage.Count;
                    result.AddRange(nextPage);
                }
            }
            return result;
            
        }

        public async Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            using (var session = store.OpenAsyncSession())
            {
            }
        }

        public Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<MatchInfo>> GetAllMatchesInfo()
        {
            return Task.FromResult(new MatchInfo[] {}.AsEnumerable());
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
