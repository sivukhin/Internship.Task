using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using Internship.Models;
using Internship.Storage;

namespace Internship.Modules
{
    public class UpdateStatisticModule : IServerModule
    {
        private static readonly Regex updateServerRegex = new Regex("^/servers/(?<serverId>.*?)/info");
        private static readonly Regex addMatchRegex = new Regex("^/servers/(?<serverId>.*?)/matches/(?<endTime>.*?)");
        private readonly IStatisticStorage statisticStorage;

        public UpdateStatisticModule(IStatisticStorage storage)
        {
            statisticStorage = storage;
        }

        public async Task<IResponse> UpdateServerInfo(IRequest request, string serverId)
        {
            var serverInfo = request.Content.ParseFromJson<ServerInfo>();
            await statisticStorage.UpdateServerInfo(serverId, serverInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }

        public async Task<IResponse> AddMatchStatistic(IRequest request, string serverId,
            DateTime endTime)
        {
            var matchInfo = request.Content.ParseFromJson<MatchInfo>();

            var serverInfo = await statisticStorage.GetServerInfo(serverId);
            if (serverInfo == null)
                return new HttpResponse(HttpStatusCode.BadRequest);
            await statisticStorage.UpdateMatchInfo(serverId, endTime, matchInfo);
            return new HttpResponse(HttpStatusCode.OK);
        }

        public async Task<IRequest> ProcessRequest(IRequest request)
        {
            if (request.HttpMethod != HttpMethodEnum.Put)
                return await Task.FromResult(request);

            var updateServerMatch = request.MatchLocalPath(updateServerRegex);
            if (updateServerMatch.Success)
                return request.AttachResponse(await UpdateServerInfo(request, updateServerMatch.Groups["serverId"].Value));

            var addMatchMatch = request.MatchLocalPath(addMatchRegex);
            if (addMatchMatch.Success)
                return request.AttachResponse(await AddMatchStatistic(request,
                    addMatchMatch.Groups["serverId"].Value,
                    DateTime.Parse(addMatchMatch.Groups["endTime"].Value)));

            return await Task.FromResult(request);
        }
    }
}