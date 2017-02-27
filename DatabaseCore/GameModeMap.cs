using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataCore;
using FluentNHibernate.Mapping;

namespace DatabaseCore
{
    public class GameModeMap : ClassMap<GameMode>
    {
        public GameModeMap()
        {
            Id(mode => mode.ModeName);
        }
    }
}
