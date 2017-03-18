﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataCore;
using HttpServerCore;
using NLog;
using StatisticServer.Storage;

namespace StatisticServer.Modules
{
    public class GetStatisticModule : BaseModule
    {
        private Logger logger;
        protected override Logger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/servers/(?<serverId>[^/]*)/info$", RegexOptions.Compiled), 
                (request, match) => GetServerInfo(match.Groups["serverId"].Value)),

            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/servers/info$", RegexOptions.Compiled), 
                request => GetAllServersInfo()),

            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex("^/servers/(?<serverId>[^/]*)/matches/(?<endTime>.*)$", RegexOptions.Compiled), 
                (request, match) => GetMatchInfo(match.Groups["serverId"].Value, DateTime.Parse(match.Groups["endTime"].Value))),  
        };

        private readonly IStatisticStorage statisticStorage;
        public GetStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public async Task<IResponse> GetServerInfo(string serverId)
        {
            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new HttpResponse(HttpStatusCode.NotFound);
            return new JsonHttpResponse(HttpStatusCode.OK, serverInfo);
        }

        public async Task<IResponse> GetAllServersInfo()
        {
            var allServersInfo = await statisticStorage.GetAllServersInfo();
            return new JsonHttpResponse(HttpStatusCode.OK, allServersInfo.Select(server => new
            {
                endpoint = server.Id,
                info = server
            }));
        }

        public async Task<IResponse> GetMatchInfo(string serverId, DateTime endTime)
        {
            var matchInfo = await statisticStorage.GetMatchInfo(serverId, endTime);
            if (matchInfo == null)
                return new HttpResponse(HttpStatusCode.NotFound);
            return new JsonHttpResponse(HttpStatusCode.OK, matchInfo);
        }
    }
}
