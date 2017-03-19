using System.Linq;
using DataCore;
using NLog;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace Kontur.GameStats.Server.Storage
{
    public class ServerById : AbstractIndexCreationTask<ServerInfo>
    {
        public ServerById()
        {
            Map = servers => servers.Select(s => new ServerInfo.ServerInfoId { Id = s.Id });
        }
    }

    public class MatchByIdAndTime : AbstractIndexCreationTask<MatchInfo>
    {
        public MatchByIdAndTime()
        {
            Map = matches => matches.Select(m => new MatchInfo.MatchInfoId { ServerId = m.HostServer.Id, EndTime = m.EndTime });
        }
    }

    public static class RavenDbStore
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        public static IDocumentStore GetStore(ApplicationOptions options)
        {
            EmbeddableDocumentStore store;
            if (options.InMemory)
            {
                store = new EmbeddableDocumentStore
                {
                    RunInMemory = true,
                    UseEmbeddedHttpServer = options.AdminHttpServer,
                    Configuration =
                    {
                        Storage = {Voron = {AllowOn32Bits = true}},
                        RunInUnreliableYetFastModeThatIsNotSuitableForProduction = options.UnitTesting
                    }
                };
            }
            else
            {
                store = new EmbeddableDocumentStore
                {
                    DataDirectory = options.DatabaseDirectory,
                    UseEmbeddedHttpServer = options.AdminHttpServer
                };
            }
            if (options.AdminHttpServer)
                logger.Info($"Started RavenDb server on {store.Configuration.ServerUrl}. " +
                            $"Enabled admin http server: {options.AdminHttpServer}");

            var initializedStore = store.Initialize();
            new ServerById().Execute(initializedStore);
            new MatchByIdAndTime().Execute(initializedStore);
            return initializedStore;
        }
    }
}