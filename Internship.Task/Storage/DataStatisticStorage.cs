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
    public class DataStatisticStorage : IDataStatisticStorage, IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IDataRepository statisticStorage;
        private readonly IGlobalServerStatisticStorage globalStatisticStorage;
        private readonly IServerStatisticStorage serverStatisticStorage;
        private readonly IPlayerStatisticStorage playerStatisticStorage;
        private readonly IReportStorage reportStorage;

        public DataStatisticStorage(
            IDataRepository statisticStorage,
            IGlobalServerStatisticStorage globalStatisticStorage,
            IServerStatisticStorage serverStatisticStorage, 
            IPlayerStatisticStorage playerStatisticStorage, 
            IReportStorage reportStorage)
        {
            logger.Info("Initialize full statistic storage");

            this.statisticStorage = statisticStorage;
            this.globalStatisticStorage = globalStatisticStorage;
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
            foreach (var server in await statisticStorage.GetAllServers())
            {
                InsertServer(server);
                processedServers++;
            }
            logger.Info($"Successfully processed {processedServers} servers entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");

            foreach (var match in await statisticStorage.GetAllMatches())
            {
                InsertMatch(match);
                processedMatches++;
                processedPlayers += match.Scoreboard.Count;
            }
            logger.Info($"Successfully processed {processedMatches} match entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");
            logger.Info($"Successfully processed {processedPlayers} players entries (elapsed {stopwatch.ElapsedMilliseconds} ms)");
        }

        public async Task UpdateServer(ServerInfo.ServerInfoId serverId, ServerInfo server)
        {
            logger.ConditionalTrace("Update information about server {0}", server);

            await statisticStorage.UpdateServer(serverId, server);
            InsertServer(server);
        }

        private void InsertServer(ServerInfo info)
        {
            Task.Factory.StartNew(() => reportStorage.Update(info));
        }

        public async Task<ServerInfo> GetServer(ServerInfo.ServerInfoId serverId)
        {
            logger.ConditionalTrace("Retrive information about server {0}", new {ServerId = serverId});

            return await statisticStorage.GetServer(serverId);
        }

        public Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            logger.ConditionalTrace("Retrieve information about all servers");

            return Task.FromResult(reportStorage.AllServers());
        }

        public async Task UpdateMatch(MatchInfo.MatchInfoId matchId, MatchInfo match)
        {
            logger.ConditionalTrace("Update information about match {0}",
                new {MatchId = matchId, MatchInfo = match});

            var server = await statisticStorage.GetServer(new ServerInfo.ServerInfoId {Id = matchId.ServerId});
            if (server == null)
                return;
            match.HostServer = server;
            match = match.InitPlayers(match.EndTime);

            var oldMatchInfo = await statisticStorage.GetMatch(match.GetIndex());
            if (oldMatchInfo != null)
                DeleteMatch(oldMatchInfo.InitPlayers(oldMatchInfo.EndTime));
            await statisticStorage.UpdateMatch(matchId, match);
            InsertMatch(match);
        }

        private void DeleteMatch(MatchInfo matchInfo)
        {
            globalStatisticStorage.Delete(matchInfo);
            serverStatisticStorage.Delete(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Delete(player);
            UpdateReports(matchInfo);
        }

        private void InsertMatch(MatchInfo matchInfo)
        {
            globalStatisticStorage.Add(matchInfo);
            serverStatisticStorage.Add(matchInfo);
            foreach (var player in matchInfo.Scoreboard)
                playerStatisticStorage.Add(player);
            UpdateReports(matchInfo);
        }

        private void UpdateReports(MatchInfo matchInfo)
        {
            Task.Factory.StartNew(() => reportStorage.Update(matchInfo))
                            .ContinueWith(_ => reportStorage.Update(matchInfo.HostServer))
                            .ContinueWith(_ =>
                            {
                                foreach (var player in matchInfo.Scoreboard)
                                    reportStorage.Update(player);
                            });
        }

        public async Task<MatchInfo> GetMatch(MatchInfo.MatchInfoId matchId)
        {
            logger.ConditionalTrace("Retrieve information about match {0}", new {MatchId = matchId});

            return await statisticStorage.GetMatch(matchId);
        }

        public async Task<IEnumerable<MatchInfo>> GetAllMatches()
        {
            logger.ConditionalTrace("Retrieve information about all matches");

            return await statisticStorage.GetAllMatches();
        }

        public void Dispose()
        {
            statisticStorage.Dispose();
        }
    }
}
