using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FluentNHibernate.Mapping;

namespace DatabaseCore
{
    class PlayerInfoMap : ClassMap<PlayerInfo>
    {
        public PlayerInfoMap()
        {
            Id(info => info.PlayerId);
            Map(info => info.Name);
            Map(info => info.Deaths);
            Map(info => info.Frags);
            Map(info => info.Kills);
            References(info => info.BaseMatch);
        }
    }
}
