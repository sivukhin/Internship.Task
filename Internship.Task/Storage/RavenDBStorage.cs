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
    public class RavenDbStorage : IStatisticStorage, IDisposable
    {
        private readonly IDocumentStore store;
        private const int PageSize = 1024;
        private const int SessionQueryLimit = 30;

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

        public async Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            return await GetAllItems<ServerInfo>();
        }

        public async Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            using (var session = store.OpenAsyncSession())
            {
                var existed = await GetMatchInfo(serverId, endTime);
                if (existed != null)
                    matchInfo.Id = existed.Id;
                await session.StoreAsync(matchInfo);
                await session.SaveChangesAsync();
            }
        }

        public async Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
            using (var session = store.OpenAsyncSession())
            {
                return await session
                    .Query<Match_ByIdAndTime.Result, Match_ByIdAndTime>()
                    .Where(m => m.ServerId == serverId && m.EndTime == endTime)
                    .OfType<MatchInfo>()
                    .FirstOrDefaultAsync();
            }
        }

        public async Task<IEnumerable<MatchInfo>> GetAllMatchesInfo()
        {
            return (await GetAllItems<MatchInfo>()).Select(match => match.InitPlayers(match.EndTime));
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}
