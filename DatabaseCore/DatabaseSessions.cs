using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using DataCore;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

namespace DatabaseCore
{
    public class Server_ById : AbstractIndexCreationTask<ServerInfo>
    {
        public Server_ById()
        {
            Map = servers => servers.Select(s => new {ServerId = s.Id});
        }
    }

    public class Match_ById : AbstractIndexCreationTask<MatchInfo>
    {
        public Match_ById()
        {
            Map = matches => matches.Select(m => new {ServerId = m.HostServer.Id, m.EndTime});
        }
    }

    public static class DatabaseConnection
    {
        public static IDocumentStore GetStore()
        {
            IDocumentStore store = new EmbeddableDocumentStore
            {
                DataDirectory = "database",
                UseEmbeddedHttpServer = true,
            };
            store = store.Initialize();
            return store;
        }
    }   
}