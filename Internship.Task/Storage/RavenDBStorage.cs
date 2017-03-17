using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                return await session.LoadAsync<ServerInfo>("ServerInfos/" + serverId);
            }
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            using (var session = store.OpenAsyncSession())
            {
                return await session.Query<ServerInfo>().ToListAsync();
            }
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
