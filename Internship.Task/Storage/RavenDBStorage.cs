using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DatabaseCore;
using DataCore;
using Raven.Client;

namespace StatisticServer.Storage
{
    public class RavenDbStorage : IDataRepository
    {
        private readonly IDocumentStore store;
        private const int PageSize = 1024;
        private const int SessionQueryLimit = 30;

        public RavenDbStorage(IDocumentStore store)
        {
            this.store = store;
        }

        public async Task UpdateServer(ServerInfo.ServerInfoId serverId, ServerInfo server)
        {
            using (var session = store.OpenAsyncSession())
            {
                await session.StoreAsync(server);
                await session.SaveChangesAsync();
            }
        }

        public async Task<ServerInfo> GetServer(ServerInfo.ServerInfoId serverId)
        {
            using (var session = store.OpenAsyncSession())
            {
                return await session.Query<ServerInfo, Server_ById>().Where(s => s.Id == serverId.Id).SingleOrDefaultAsync();
            }
        }

        private async Task<List<T>> GetSomePages<T>(int offset)
        {
            var result = new List<T>();
            using (var session = store.OpenAsyncSession())
            {
                for (int i = 0; i < SessionQueryLimit; i++)
                {
                    RavenQueryStatistics queryStat;
                    var page = await session.Query<T>()
                        .Statistics(out queryStat)
                        .Skip(offset)
                        .Take(PageSize)
                        .ToListAsync();
                    result.AddRange(page);
                    offset += page.Count;
                    if (offset == queryStat.TotalResults)
                        break;
                }
            }
            return result;
        }

        private async Task<IEnumerable<T>> GetAllItems<T>()
        {
            int offset = 0;
            var result = new List<T>();
            while (true)
            {
                var pages = await GetSomePages<T>(offset);
                result.AddRange(pages);
                offset += pages.Count;
                if (pages.Count == 0)
                    break;
            }
            return result;
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            return await GetAllItems<ServerInfo>();
        }

        public async Task UpdateMatch(MatchInfo.MatchInfoId matchId, MatchInfo match)
        {
            using (var session = store.OpenAsyncSession())
            {
                var existed = await GetMatch(match.GetIndex());
                if (existed != null)
                    match.Id = existed.Id;
                await session.StoreAsync(match);
                await session.SaveChangesAsync();
            }
        }

        public async Task<MatchInfo> GetMatch(MatchInfo.MatchInfoId matchId)
        {
            using (var session = store.OpenAsyncSession())
            {
                return await session
                    .Query<MatchInfo.MatchInfoId, Match_ByIdAndTime>()
                    .Where(m => m.ServerId == matchId.ServerId && m.EndTime == matchId.EndTime)
                    .OfType<MatchInfo>()
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<MatchInfo>> GetAllMatches()
        {
            return (await GetAllItems<MatchInfo>()).Select(match => match.InitPlayers(match.EndTime));
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
