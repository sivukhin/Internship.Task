using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataCore;

namespace Kontur.GameStats.Server.Storage
{
    public interface IDataRepository : IDisposable
    {
        Task UpdateServer(ServerInfo.ServerInfoId serverId, ServerInfo server);
        Task<ServerInfo> GetServer(ServerInfo.ServerInfoId serverId);
        Task<IEnumerable<ServerInfo>> GetAllServers();

        Task UpdateMatch(MatchInfo.MatchInfoId matchId, MatchInfo match);
        Task<MatchInfo> GetMatch(MatchInfo.MatchInfoId matchId);
        Task<IEnumerable<MatchInfo>> GetAllMatches();
    }
}