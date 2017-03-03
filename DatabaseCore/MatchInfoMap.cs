using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FluentNHibernate.Mapping;

namespace DatabaseCore
{
    class MatchInfoMap : ClassMap<MatchInfo>
    {
        public MatchInfoMap()
        {
            Id(info => info.MatchId);
            References(info => info.GameMode);
            Map(info => info.ElapsedTime);
            Map(info => info.EndTime);
            Map(info => info.FragLimit);
            Map(info => info.TimeLimit);
            Map(info => info.Map);
            References(info => info.HostServer);
            HasMany(info => info.Scoreboard).Cascade.SaveUpdate().Not.LazyLoad();
        }
    }
}
