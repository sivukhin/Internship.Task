﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using DatabaseCore;
using DataCore;
using HttpServerCore;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var storage = new SQLiteStorage(DatabaseSessions.CreateSessionFactory());
            using (var server = new HttpServer(
                new HttpServerOptions {Prefix = "http://localhost:12345/"},
                new IServerModule[]
                {
                    new UpdateStatisticModule(storage),
                    new GetStatisticModule(storage),
                    new StatsModule(storage), 
                }))
            {
                server.Start();
                Console.ReadKey(true);
            }
        }
    }
}