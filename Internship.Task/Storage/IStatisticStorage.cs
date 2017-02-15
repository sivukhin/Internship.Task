using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Internship.Models;

namespace Internship.Storage
{
    public interface IStatisticStorage
    {
        Task UpdateServerInfo(string serverId, ServerInfo info);
        Task<ServerInfo> GetServerInfo(string serverId);
        Task<IEnumerable<ServerInfo>> GetAllServersInfo();

        Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo);
        Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime);
    }
}