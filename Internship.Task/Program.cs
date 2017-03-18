using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Threading;
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
                using (var storage = new RavenDbStorage(DatabaseConnection.GetStore()))
                {
                    var statisticStorage = new FullStatisticStorage(storage, serverStatistics, playerStatistics,
                        reportStorage);
                    using (var server = new HttpServer(
                        new HttpServerOptions {Prefix = "http://127.0.0.1:12345/"},
                        new IServerModule[]
                        {
                            new UpdateStatisticModule(statisticStorage),
                            new GetStatisticModule(statisticStorage),
                            new StatsModule(serverStatistics, playerStatistics),
                            new ReportsModule(reportStorage),
                        }))
                    {
                        server.Start();
                        Thread.CurrentThread.Join();
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Application crashed");
            }
        }
    }
}