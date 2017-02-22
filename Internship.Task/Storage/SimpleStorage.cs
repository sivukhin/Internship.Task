using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatisticServer.Models;

namespace StatisticServer.Storage
{
    public class SimpleStorage : IStatisticStorage
    {
        private readonly Dictionary<Tuple<string, DateTime>, MatchInfo> matchInfo;
        private readonly Dictionary<string, ServerInfo> serverInfo;
        private static Task EmptyTask => Task.FromResult(0);

        public SimpleStorage()
        {
            serverInfo = new Dictionary<string, ServerInfo>();
            matchInfo = new Dictionary<Tuple<string, DateTime>, MatchInfo>();
        }

        public Task UpdateServerInfo(string serverId, ServerInfo info)
        {
            serverInfo[serverId] = info;
            return EmptyTask;
        }

        public Task<ServerInfo> GetServerInfo(string serverId)
        {
            ServerInfo info;
            serverInfo.TryGetValue(serverId, out info);
            return Task.FromResult(info);
        }

        public Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo match)
        {
            matchInfo[Tuple.Create(serverId, endTime)] = match;
            return EmptyTask;
        }

        public Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
            MatchInfo info;
            matchInfo.TryGetValue(Tuple.Create(serverId, endTime), out info);
            return Task.FromResult(info);
        }

        public Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            return Task.FromResult((IEnumerable<ServerInfo>)serverInfo.Values);
        }
    }
}