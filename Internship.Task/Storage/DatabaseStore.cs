using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using DataCore;
using Raven.Abstractions.Replication;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace StatisticServer.Storage
{
    public class Server_ById : AbstractIndexCreationTask<ServerInfo>
    {
        public Server_ById()
        {
            Map = servers => servers.Select(s => new ServerInfo.ServerInfoId { Id = s.Id });
        }
    }

    public class Match_ByIdAndTime : AbstractIndexCreationTask<MatchInfo>
    {
        public Match_ByIdAndTime()
        {
            Map = matches => matches.Select(m => new MatchInfo.MatchInfoId { ServerId = m.HostServer.Id, EndTime = m.EndTime });
        }
    }

    public static class RaveDbStore
    {
        public static IDocumentStore GetStore(ApplicationOptions options)
        {
            IDocumentStore store;
            if (options.InMemory)
            {
                store = new EmbeddableDocumentStore
                {
                    RunInMemory = true,
                    UseEmbeddedHttpServer = options.AdminHttpServer,
                    Configuration = {Storage = {Voron = {AllowOn32Bits = true}}}
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
            store = store.Initialize();
            new Server_ById().Execute(store);
            new Match_ByIdAndTime().Execute(store);
            return store;
        }
    }
}