using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatCore.Stats;

namespace StatisticServer.Storage
{
    public abstract class BaseStatisticsProvider<TTarget>
    {
        private readonly IList<IStatStorage<TTarget>> stats;
        protected BaseStatisticsProvider()
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