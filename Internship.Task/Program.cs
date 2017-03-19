using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Autofac;
using Fclp;
using HttpServerCore;
using NLog;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Serialization;
using StatisticServer.Modules;
using StatisticServer.Storage;

namespace StatisticServer
{
    public class ApplicationOptions
    {
        public const string DefaultDatabaseDirectory = "database";

        public string Prefix { get; set; }

        private string databaseDirectiory;

        public string DatabaseDirectory
        {
            get { return databaseDirectiory ?? DefaultDatabaseDirectory; }
            set { databaseDirectiory = value; }
        }

        public bool AdminHttpServer { get; set; }
        public bool EnableLogs { get; set; }
        public bool InMemory { get; set; }

        public bool UnitTesting { get; set; }

        private void ValidatePrefix()
        {
            var wellFormedUri = new Regex("[*+]").Replace(Prefix, "localhost");
            if (!Uri.IsWellFormedUriString(wellFormedUri, UriKind.Absolute))
                throw new ArgumentException(
                    "Incorrect Uri Prefix. " +
                    "Go to the https://msdn.microsoft.com/en-us/library/windows/desktop/aa364698(v=vs.85).aspx " +
                    "for pretty good specification of valid Uri prefix");
        }

        public void Validate()
        {
            ValidatePrefix();
        }
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
            options.Validate();
            RunApplication(options);
        }

        private static void RunApplication(ApplicationOptions options)
        {
            ConfigureApplication(options);
            logger.Info("Application started");
            var container = CompositionRoot(options);
            using (var lifetimeScope = container.BeginLifetimeScope())
            {
                var server = lifetimeScope.Resolve<IHttpServer>();
                server.Start();
                Console.Error.WriteLine("Press Ctrl+C for interrupt server");
                Thread.CurrentThread.Join();
            }
        }

        private static void ConfigureApplication(ApplicationOptions options)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            if (!options.EnableLogs)
                LogManager.Configuration.FindTargetByName("ColoredConsole").Dispose();
        }

        private static IContainer CompositionRoot(ApplicationOptions options)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<PlayerStatisticStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ServerStatisticStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<ReportStorage>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterType<GlobalServerStatisticStorage>().AsImplementedInterfaces().SingleInstance();

            builder.RegisterInstance(new RavenDbStorage(RavenDbStore.GetStore(options)))
                .As<IDataRepository>()
                .SingleInstance();

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
                .WithDescription($"Set database directory. Default directory: '{ApplicationOptions.DefaultDatabaseDirectory}'");
            parser.Setup(arg => arg.AdminHttpServer)
                .As("admin_http")
                .SetDefault(false)
                .WithDescription("Enable RavenDB default embedded http server");
            parser.Setup(arg => arg.EnableLogs)
                .As("logs")
                .SetDefault(false)
                .WithDescription("Enable verbose console logging");
            parser.Setup(arg => arg.InMemory)
                .As("in_mem")
                .SetDefault(false)
                .WithDescription("Run RavenDB in memory");

            parser.SetupHelp("h", "help").Callback(text => Console.WriteLine(text));

            return parser;
        }
    }
}