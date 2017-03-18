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
    public class ApplicationOptions
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

        private static void RunApplication(ApplicationOptions options)
        {
            logger.Info("Application started");
            ConfigureApplication();
            var container = CompositionRoot(options);
            using (var lifetimeScope = container.BeginLifetimeScope())
            {
                var server = lifetimeScope.Resolve<IHttpServer>();
                server.Start();
                Console.Error.WriteLine("Press Ctrl+C for interrupt server");
                Thread.CurrentThread.Join();
            }
        }

        private static void ConfigureApplication()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static IContainer CompositionRoot(ApplicationOptions options)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<PlayerStatisticStorage>().AsImplementedInterfaces();
            builder.RegisterType<ServerStatisticStorage>().AsImplementedInterfaces();
            builder.RegisterType<ReportStorage>().AsSelf();
            builder.RegisterInstance(new RavenDbStorage(DatabaseConnection.GetStore())).As<IDataRepository>();
            builder.RegisterType<DataStatisticStorage>().As<IDataStatisticStorage>().SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(BaseModule)))
                .Where(t => t.IsAssignableTo<IServerModule>())
                .AsImplementedInterfaces();
            builder.RegisterType<HttpServer>().AsImplementedInterfaces();
            builder.RegisterInstance(new HttpServerOptions {Prefix = options.Prefix}).AsSelf();

            return builder.Build();
        }

        private static bool ShouldTerminate(ICommandLineParserResult result)
        {
            if (result.HasErrors)
                Console.Error.WriteLine(result.ErrorText);
            return result.HasErrors || result.HelpCalled;
        }

        private static FluentCommandLineParser<ApplicationOptions> ConfigureParser()
        {
            var parser = new FluentCommandLineParser<ApplicationOptions>();
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
    }
}