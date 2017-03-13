﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using StatCore;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public class ServerStatistic
    {
        public int TotalMatchesPlayed { get; set; }
        public int MaximumMatchesPerDay { get; set; }
        public double AverageMatchesPerDay { get; set; }
        public int MaximumPopulation { get; set; }
        public double AveragePopulation { get; set; }
        public IEnumerable<string> Top5GameModes { get; set; }
        public IEnumerable<string> Top5Maps { get; set; }
    }
    
    public interface IServerStatisticStorage
    {
        ServerStatistic GetStatistics(string serverId);
        void Add(MatchInfo player);
        void Delete(MatchInfo player);
    }

    public class ServerStatisticStorage : BaseStatisticStorage<MatchInfo>, IServerStatisticStorage
    {
        private static GroupedStat<MatchInfo, T, string> CreateStat<T>
            (Func<DataIdentity<MatchInfo>, IStat<MatchInfo, T>> statFactory)
        {
            return new GroupedStat<MatchInfo, T, string>(server => server.HostServer.ServerId, () => statFactory(Info));
        }

        private readonly GroupedStat<MatchInfo, int, string> totalMatchesPlayed =
            CreateStat(match => match.Count());

        private readonly GroupedStat<MatchInfo, int, string> maximumMatchesPerDay =
            CreateStat(match => match.Split(info => info.EndTime.Date, splitted => splitted.Count()).Max());

        private readonly IStat<MatchInfo, DateTime> firstDayWithMatch = Info.Min(match => match.EndTime.Date);

        private readonly IStat<MatchInfo, DateTime> lastDayWithMatch = Info.Max(match => match.EndTime.Date);

        private readonly GroupedStat<MatchInfo, int, string> maximumPopulation =
            CreateStat(match => match.Max(info => info.Scoreboard.Count));

        private readonly GroupedStat<MatchInfo, double, string> averagePopulation =
            CreateStat(match => match.Average(info => info.Scoreboard.Count));

        private readonly GroupedStat<MatchInfo, IEnumerable<string>, string> top5GameModes =
            CreateStat(match => match.Select(info => info.GameMode.ModeName).Popular(5));

        private readonly GroupedStat<MatchInfo, IEnumerable<string>, string> top5Maps =
            CreateStat(match => match.Select(info => info.Map).Popular(5));

        public double AverageMatchesPerDay(string serverId)
        {
            return 1.0 * totalMatchesPlayed[serverId] / ((lastDayWithMatch.Value - firstDayWithMatch.Value).Days + 1);
        }

        public ServerStatistic GetStatistics(string serverId)
        {
            if (totalMatchesPlayed[serverId] == 0)
                return null;
            return new ServerStatistic
            {
                TotalMatchesPlayed = totalMatchesPlayed[serverId],
                MaximumMatchesPerDay = maximumMatchesPerDay[serverId],
                AverageMatchesPerDay = AverageMatchesPerDay(serverId),
                MaximumPopulation = maximumPopulation[serverId],
                Top5GameModes = top5GameModes[serverId],
                Top5Maps = top5Maps[serverId],
                AveragePopulation = averagePopulation[serverId]
            };
        }
    }
}