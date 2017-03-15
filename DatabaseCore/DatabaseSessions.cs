using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using DataCore;
using FluentNHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace DatabaseCore
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        public override bool IsId(Member member)
        {
            return member.Name.EndsWith("Id");
        }

        public override bool ShouldMap(Type type)
        {
            return type.IsPublic;
        }
    }

    public class PlayerInfoOverride : IAutoMappingOverride<PlayerInfo>
    {
        public void Override(AutoMapping<PlayerInfo> mapping)
        {
            mapping.Id(x => x.PlayerId);
            mapping.IgnoreProperty(info => info.ScoreboardPercent);
            mapping.IgnoreProperty(info => info.AreWinner);
        }
    }

    public class MatchInfoOverride : IAutoMappingOverride<MatchInfo>
    {
        public void Override(AutoMapping<MatchInfo> mapping)
        {
            mapping.HasMany(info => info.Scoreboard).Cascade.SaveUpdate().Not.LazyLoad();
        }
    }

    public static class DatabaseSessions
    {
        private static string dbFileName = "statistic_server.sqlite";
        private static string dbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName);

        public static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(dbFilePath))
                .Mappings(configuration =>
                {
                    configuration.AutoMappings.Add(
                        () =>
                            AutoMap.AssemblyOf<PlayerInfo>(new AutomappingConfiguration())
                            .UseOverridesFromAssemblyOf<PlayerInfoOverride>());
                })
                .ExposeConfiguration(BuildSchema)
                .BuildSessionFactory();
        }

        private static void BuildSchema(Configuration configuration)
        {
            //TODO: Pass options from CLI 
            if (File.Exists(dbFilePath))
                File.Delete(dbFilePath);
            new SchemaExport(configuration).Create(true, true);
        }
    }
}