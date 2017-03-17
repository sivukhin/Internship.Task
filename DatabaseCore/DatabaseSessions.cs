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
            Map = servers => servers.Select(s => new {s.Id});
        }
    }

    public class Match_ByIdAndTime : AbstractIndexCreationTask<MatchInfo>
    {
        public class Result
        {
            public string ServerId { get; set; }
            public DateTime EndTime { get; set; }
        }
        public Match_ByIdAndTime()
        {
            Map = matches => matches.Select(m => new {ServerId = m.HostServer.Id, EndTime = m.EndTime});
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
            new Server_ById().Execute(store);
            new Match_ByIdAndTime().Execute(store);
            return store;
        }
    }   
}