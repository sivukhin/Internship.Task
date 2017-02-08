using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Internship.Models;
using Internship.Storage;

namespace Internship.Modules
{
    public class GetStatisticModule : BaseServerModule
    {
        private IStatisticStorage statisticStorage;
        public GetStatisticModule(IStatisticStorage statisticStorage)
        {
            this.statisticStorage = statisticStorage;
        }

        [RequestFilter(HttpMethodEnum.Get, "/servers/(?<endpoint>.*?)/info")]
        public async Task<IHandlerResult> GetServerInfo(HttpListenerContext context, string endpoint)
        {
            var serverInfo = await statisticStorage.GetServerInfo(endpoint);
            if (serverInfo == null)
                return new BaseHandlerResult(HttpStatusCode.NotFound);
            return new JsonHandlerResult<ServerInfo>(HttpStatusCode.OK, serverInfo);
        }

        [RequestFilter(HttpMethodEnum.Get, "/servers/info")]
        public async Task<IHandlerResult> GetServersInfo(HttpListenerContext context)
        {
            var serversInfo = await statisticStorage.GetAllServersInfo();
            var enumerated = serversInfo.ToList();
            return new JsonHandlerResult<List<ServerInfo>>(HttpStatusCode.OK, enumerated);
        }

        [RequestFilter(HttpMethodEnum.Get, "/servers/(?<endpoint>)/matches/(?<timestamp>)")]
        public async Task<IHandlerResult> GetMatchInfo(HttpListenerContext context, string endpoint, string timestamp)
        {
            var matchInfo = await statisticStorage.GetMatchInfo(endpoint, DateTime.Parse(timestamp));
            if (matchInfo == null)
                return new BaseHandlerResult(HttpStatusCode.NotFound);
            return new JsonHandlerResult<MatchInfo>(HttpStatusCode.OK, matchInfo);
        }
    }
}
