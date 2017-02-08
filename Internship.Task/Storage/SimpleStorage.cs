using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Internship.Models;

namespace Internship.Storage
{
    public class SimpleStorage : IStatisticStorage
    {
        private Dictionary<Tuple<string, DateTime>, MatchInfo> matchInfo;
        private readonly Dictionary<string, ServerInfo> serverInfo;

        public SimpleStorage()
        {
            serverInfo = new Dictionary<string, ServerInfo>();
            matchInfo = new Dictionary<Tuple<string, DateTime>, MatchInfo>();
        }

        public async Task UpdateServerInfo(string serverName, ServerInfo info)
        {
            serverInfo[serverName] = info;
        }

        public async Task<ServerInfo> GetServerInfo(string serverName)
        {
            ServerInfo info;
            serverInfo.TryGetValue(serverName, out info);
            return info;
        }

        public async Task UpdateMatchInfo(string serverName, DateTime endTime, MatchInfo match)
        {
            matchInfo[Tuple.Create(serverName, endTime)] = match;
        }

        public async Task<MatchInfo> GetMatchInfo(string serverName, DateTime endTime)
        {
            MatchInfo info;
            matchInfo.TryGetValue(Tuple.Create(serverName, endTime), out info);
            return info;
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            return serverInfo.Values;
        }
    }
}