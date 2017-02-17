using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using Internship.Storage;

namespace Internship.Modules
{
    public class GetStatisticModule : IServerModule
    {
        private static readonly Regex getServerInfoRegex = new Regex("^/servers/(?<serverId>.*?)/info");
        private static readonly Regex getAllServersInfoRegex = new Regex("^/servers/info");
        private static readonly Regex getMatchInfoRegex = new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)");

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
            return new JsonHttpResponse(HttpStatusCode.OK, allServersInfo);
        }

        public async Task<IResponse> GetMatchInfo(string serverId, DateTime endTime)
        {
            var matchInfo = await statisticStorage.GetMatchInfo(serverId, endTime);
            if (matchInfo == null)
                return new HttpResponse(HttpStatusCode.NotFound);
            return new JsonHttpResponse(HttpStatusCode.OK, matchInfo);
        }

        public async Task<IRequest> ProcessRequest(IRequest request)
        {
            if (request.HttpMethod != HttpMethodEnum.Get)
                return await Task.FromResult(request);
            var getServerInfoMatch = request.MatchLocalPath(getServerInfoRegex);
            if (getServerInfoMatch.Success)
                return request.AttachResponse(await GetServerInfo(getServerInfoMatch.Groups["serverId"].Value));

            var getAllServersInfoMatch = request.MatchLocalPath(getAllServersInfoRegex);
            if (getAllServersInfoMatch.Success)
                return request.AttachResponse(await GetAllServersInfo());

            var getMatchInfoMatch = request.MatchLocalPath(getMatchInfoRegex);
            if (getMatchInfoMatch.Success)
                return request.AttachResponse(await GetMatchInfo(
                    getMatchInfoMatch.Groups["serverId"].Value,
                    DateTime.Parse(getMatchInfoMatch.Groups["endTime"].Value)));

            return await Task.FromResult(request);
        }
    }
}
