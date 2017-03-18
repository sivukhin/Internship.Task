using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using NLog;

namespace StatisticServer.Storage
{
    public class FullStatisticStorage : IStatisticStorage
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IStatisticStorage statisticStorage;
        private readonly IServerStatisticStorage serverStatisticStorage;
        private readonly IPlayerStatisticStorage playerStatisticStorage;
        private readonly IAggregateReportStorage reportStorage;

        public FullStatisticStorage(
            IStatisticStorage statisticStorage,
            IServerStatisticStorage serverStatisticStorage, 
            IPlayerStatisticStorage playerStatisticStorage, 
            IAggregateReportStorage reportStorage)
        {
            logger.Info("Initialize full statistic storage");

            this.statisticStorage = statisticStorage;
            this.serverStatisticStorage = serverStatisticStorage;
            this.playerStatisticStorage = playerStatisticStorage;
            this.reportStorage = reportStorage;
            InitStatisticsProviders();
        }

        private void InitStatisticsProviders()
        {
            InitServerStatisticsProvider().Wait();
        }

        private async Task InitServerStatisticsProvider()
        {
            var stopwatch = new Stopwatch().Run();

            logger.Info("Initialize server statistics from database");
            int processedMatches = 0, processedPlayers = 0, processedServers = 0;
            foreach (var server in await statisticStorage.GetAllServersInfo())
            {
                InsertServer(server);
                processedServers++;
            }
            logger.Info($"Successfully processed {processedServers} servers entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");

            foreach (var match in await statisticStorage.GetAllMatchesInfo())
            {
                InsertMatch(match);
                processedMatches++;
                processedPlayers += match.Scoreboard.Count;
            }
            logger.Info($"Successfully processed {processedMatches} match entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");
            logger.Info($"Successfully processed {processedPlayers} players entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");
        }

        public async Task UpdateServerInfo(string serverId, ServerInfo info)
        {
            info.Id = serverId;
            logger.Trace("Update information about server {0}", info);

            InsertServer(info);
            await statisticStorage.UpdateServerInfo(serverId, info);
        }

        private void InsertServer(ServerInfo info)
        {
            Task.Factory.StartNew(() => reportStorage.Update(info));
        }

        public async Task<ServerInfo> GetServerInfo(string serverId)
        {
            logger.Trace("Retrive information about server {0}", new {ServerId = serverId});

            return await statisticStorage.GetServerInfo(serverId);
        }

        public Task<IEnumerable<ServerInfo>> GetAllServersInfo()
        {
            logger.Trace("Retrieve information about all servers");

            return Task.FromResult(reportStorage.AllServers());
        }

        public async Task UpdateMatchInfo(string serverId, DateTime endTime, MatchInfo matchInfo)
        {
            logger.Trace("Update information about match {0}",
                new {ServerId = serverId, EndTime = endTime, MatchInfo = matchInfo});

            var server = await statisticStorage.GetServerInfo(serverId);
            if (server == null)
                return;
            matchInfo.HostServer = server;
            var oldMatchInfo = await statisticStorage.GetMatchInfo(serverId, endTime);
            if (oldMatchInfo != null)
                DeleteMatch(oldMatchInfo.InitPlayers(endTime));
            InsertMatch(matchInfo);
            await statisticStorage.UpdateMatchInfo(serverId, endTime, matchInfo);
        }

        private void DeleteMatch(MatchInfo matchInfo)
        {
            serverStatisticStorage.Delete(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Delete(player);
            Task.Factory.StartNew(() => reportStorage.Update(matchInfo))
                .ContinueWith(_ => reportStorage.Update(matchInfo.HostServer))
                .ContinueWith(_ =>
                {
                    foreach (var player in matchInfo.Scoreboard)
                        reportStorage.Update(player);
                });
        }

        private void InsertMatch(MatchInfo matchInfo)
        {
            serverStatisticStorage.Add(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Add(player);
            Task.Factory.StartNew(() => reportStorage.Update(matchInfo))
                .ContinueWith(_ => reportStorage.Update(matchInfo.HostServer))
                .ContinueWith(_ =>
                {
                    foreach (var player in matchInfo.Scoreboard)
                        reportStorage.Update(player);
                });
        }

        public async Task<MatchInfo> GetMatchInfo(string serverId, DateTime endTime)
        {
            logger.Trace("Retrieve information about match {0}", new {ServerId = serverId, EndTime = endTime});

            return await statisticStorage.GetMatchInfo(serverId, endTime);
        }

        public async Task<IEnumerable<MatchInfo>> GetAllMatchesInfo()
        {
            logger.Trace("Retrieve information about all matches");

            return await statisticStorage.GetAllMatchesInfo();
        }
    }
}
