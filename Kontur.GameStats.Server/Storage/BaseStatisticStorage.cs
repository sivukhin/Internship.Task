using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatCore.DataFlow;
using StatCore.Stats;

namespace Kontur.GameStats.Server.Storage
{
    public abstract class BaseStatisticStorage<TTarget>
    {
        private readonly IList<IStatStorage<TTarget>> stats;
        protected static DataIdentity<TTarget> Info => new DataIdentity<TTarget>();

        protected BaseStatisticStorage()
        {
            stats = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(field => typeof(IStatStorage<TTarget>).IsAssignableFrom(field.FieldType))
                .Select(field => field.GetValue(this))
                .Cast<IStatStorage<TTarget>>()
                .ToList();
        }

        public void Add(TTarget value)
        {
            foreach (var stat in stats)
                stat.Add(value);
        }

        public void Delete(TTarget value)
        {
            foreach (var stat in stats)
                stat.Delete(value);
        }
    }
}