using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Internship.Models;

namespace Internship.Storage
{
    public interface IStatisticStorage
    {
        Task UpdateServerInfo(string serverName, ServerInfo info);
        Task<ServerInfo> GetServerInfo(string serverName);
        Task<IEnumerable<ServerInfo>> GetAllServersInfo();

        Task UpdateMatchInfo(string serverName, DateTime endTime, MatchInfo matchInfo);
        Task<MatchInfo> GetMatchInfo(string serverName, DateTime endTime);
    }
}