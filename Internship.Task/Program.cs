using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using DatabaseCore;
using DataCore;
using HttpServerCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Layouts;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer
{

    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static void Main(string[] args)
        {
            logger.Info("Application started");
            try
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
            catch (Exception exception)
            {
                logger.Fatal(exception, "Application crashed");
            }
        }
    }
}