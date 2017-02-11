using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Internship.Models;

namespace Internship.Storage
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

        public Task UpdateServerInfo(string serverName, ServerInfo info)
        {
            serverInfo[serverName] = info;
            return EmptyTask;
        }

        public Task<ServerInfo> GetServerInfo(string serverName)
        {
            ServerInfo info;
            serverInfo.TryGetValue(serverName, out info);
            return Task.FromResult(info);
        }

        public Task UpdateMatchInfo(string serverName, DateTime endTime, MatchInfo match)
        {
            matchInfo[Tuple.Create(serverName, endTime)] = match;
            return EmptyTask;
        }

        public Task<MatchInfo> GetMatchInfo(string serverName, DateTime endTime)
        {
            MatchInfo info;
            matchInfo.TryGetValue(Tuple.Create(serverName, endTime), out info);
            return Task.FromResult(info);
        }

        public Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            return Task.FromResult((IEnumerable<ServerInfo>)serverInfo.Values);
        }
    }
}