using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Autofac;
using Autofac.Builder;
using DatabaseCore;
using DataCore;
using Fclp;
using HttpServerCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Layouts;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer
{
    public class CommandLineOptions
    {
        public string Prefix { get; set; }
        public string DatabaseDirectory { get; set; }
        public bool ServerAdminHttpServer { get; set; }
        public bool EnableLogs { get; set; }
    }

    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            try
            {
                RunCli(args);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
                logger.Fatal(exception, "Application crashed");
            }
            finally
            {
                LogManager.Flush();
                LogManager.Shutdown();
            }
        }

        private static void RunCli(string[] args)
        {
            var parser = ConfigureParser();
            var result = parser.Parse(args);
            if (ShouldTerminate(result))
                return;
            var options = parser.Object;
            RunApplication(options);
        }

        private static void RunApplication(CommandLineOptions options)
        {
            logger.Info("Application started");
            ConfigureApplication();
            Start();
        }

        private static void ConfigureApplication()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static IContainer CompositionRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<PlayerStatisticStorage>().AsImplementedInterfaces();
            builder.RegisterType<ServerStatisticStorage>().AsImplementedInterfaces();
            builder.RegisterType<ReportStorage>().AsSelf();
            builder.RegisterInstance(new RavenDbStorage(DatabaseConnection.GetStore()));

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(BaseModule)))
                .Where(t => t.IsInstanceOfType(typeof(IServerModule)))
                .AsImplementedInterfaces();

            return builder.Build();
        }

        private static bool ShouldTerminate(ICommandLineParserResult result)
        {
            if (result.HasErrors)
                Console.Error.WriteLine(result.ErrorText);
            return result.HasErrors || result.HelpCalled;
        }

        private static FluentCommandLineParser<CommandLineOptions> ConfigureParser()
        {
            var parser = new FluentCommandLineParser<CommandLineOptions>();
            parser.Setup(arg => arg.Prefix)
                .As('p', "prefix")
                .Required()
                .WithDescription("Set address of statistic server");
            parser.Setup(arg => arg.DatabaseDirectory)
                .As('d', "database")
                .SetDefault("database")
                .WithDescription("Set database directory");
            parser.Setup(arg => arg.ServerAdminHttpServer)
                .As("admin_http")
                .SetDefault(false)
                .WithDescription("Enable RavenDB default embedded http server");
            parser.Setup(arg => arg.EnableLogs)
                .As("logs")
                .SetDefault(false)
                .WithDescription("Enable verbose console logging");

            parser.SetupHelp("h", "help").Callback(text => Console.WriteLine(text));

            return parser;
        }

        private static void Start()
        {
            var serverStatistics = new ServerStatisticStorage();
            var playerStatistics = new PlayerStatisticStorage();
            var reportStorage = new ReportStorage(serverStatistics, playerStatistics);
            using (var storage = new RavenDbStorage(DatabaseConnection.GetStore()))
            {
                var statisticStorage = new DataStatisticStorage(storage, serverStatistics, playerStatistics,
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
    }
}