using System;
using System.IO;
using DataCore;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace DatabaseCore
{
    public static class DatabaseSessions
    {
        private static string dbFileName = "statistic_server.sqlite";
        private static string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);

        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(dbFilePath))
                .Mappings(configuration => configuration.FluentMappings.Add<GameModeMap>())
                .Mappings(configuration => configuration.FluentMappings.Add<ServerInfoMap>())
                .Mappings(configuration => configuration.FluentMappings.Add<PlayerInfoMap>())
                .Mappings(configuration => configuration.FluentMappings.Add<MatchInfoMap>())
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private static void BuildSchema(Configuration configuration)
        {
            //TODO: Pass options from CLI 
            if (File.Exists(dbFilePath))
                return;//File.Delete(dbFilePath);
            new SchemaExport(configuration).Create(false, true);
        }
    }
}