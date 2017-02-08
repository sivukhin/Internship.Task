using System;
using System.Net;
using System.Threading.Tasks;
using Internship.Models;
using Internship.Storage;

namespace Internship.Modules
{
    public class UpdateStatisticsModule : BaseServerModule
    {
        private readonly IStatisticStorage statisticStorage;
        public UpdateStatisticsModule(IStatisticStorage statisticStorage)
        {
            this.statisticStorage = statisticStorage;
        }

        [RequestFilter(HttpMethodEnum.Put, "/servers/(?<endpoint>.*?)/info")]
        public async Task<IHandlerResult> UpdateServerInfo(HttpListenerContext context, string endpoint)
        {
            var serverInfo = ParseRequestJson<ServerInfo>(context.Request.InputStream);

            await statisticStorage.UpdateServerInfo(endpoint, serverInfo);
            return new BaseHandlerResult(HttpStatusCode.OK);
        }

        [RequestFilter(HttpMethodEnum.Put, "/servers/(?<endpoint>.*?)/matches/(?<timestamp>.*?)")]
        public async Task<IHandlerResult> AddMatchStatistic(HttpListenerContext context, string endpoint, string timestamp)
        {
            var matchInfo = ParseRequestJson<MatchInfo>(context.Request.InputStream);

            var serverInfo = await statisticStorage.GetServerInfo(endpoint);
            if (serverInfo == null)
                return new BaseHandlerResult(HttpStatusCode.BadRequest);
            await statisticStorage.UpdateMatchInfo(endpoint, DateTime.Parse(timestamp), matchInfo);
            return new BaseHandlerResult(HttpStatusCode.OK);
        }
    }
}