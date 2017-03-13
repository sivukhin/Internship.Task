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
                var serverStatistics = new ServerStatisticStorage();
                var playerStatistics = new PlayerStatisticStorage();
                var reportStorage = new ReportStorage(serverStatistics, playerStatistics);
                var storage = new SQLiteStorage(DatabaseSessions.CreateSessionFactory(), serverStatistics, playerStatistics, reportStorage);
                using (var server = new HttpServer(
                    new HttpServerOptions {Prefix = "http://localhost:12345/"},
                    new IServerModule[]
                    {
                        new UpdateStatisticModule(storage),
                        new GetStatisticModule(storage),
                        new StatsModule(serverStatistics, playerStatistics),
                        new ReportsModule(reportStorage), 
                    }))
                {
                    server.Start();
                    while (true)
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