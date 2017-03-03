using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataCore;

namespace StatisticServer.Storage
{
    public interface IStatisticStorage
    {
        Task UpdateServerInfo(string serverId, ServerInfo info);
        Task<ServerInfo> GetServerInfo(string serverId);
        Task<IEnumerable<ServerInfo>> GetAllServersInfo();

        Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo);
        Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime);

        Task<ServerStatistic> GetServerStatistics(string serverId);
    }
}