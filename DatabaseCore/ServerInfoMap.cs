using DataCore;
using FluentNHibernate.Mapping;

namespace DatabaseCore
{
    public class ServerInfoMap : ClassMap<ServerInfo>
    {
        public ServerInfoMap()
        {
            Id(info => info.ServerId);
            Map(info => info.Name);
            HasMany(info => info.GameModes).Cascade.SaveUpdate();
        }
    }
}