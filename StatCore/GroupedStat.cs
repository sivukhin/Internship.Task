using System;
using System.Collections.Generic;
using StatCore.Stats;

namespace StatCore
{
    public class GroupedStat<TIn, TOut, TGroup> : IStatStorage<TIn>
    {
        private readonly Func<TIn, TGroup> grouper;
        private readonly Func<IStat<TIn, TOut>> stat;
        private Dictionary<TGroup, IStat<TIn, TOut>> groupStats;

        public GroupedStat(Func<TIn, TGroup> grouper, Func<IStat<TIn, TOut>> stat)
        {
            this.grouper = grouper;
            this.stat = stat;
            groupStats = new Dictionary<TGroup, IStat<TIn, TOut>>();
        }

        public IStat<TIn, TOut> GetGroup(TIn item)
        {
            if (groupStats.ContainsKey(grouper(item)))
                return groupStats[grouper(item)];
            return groupStats[grouper(item)] = stat();
        }

        public void Add(TIn item)
        {
            GetGroup(item).Add(item);
        }

        public void Delete(TIn item)
        {
            GetGroup(item).Delete(item);
        }

        public TOut this[TGroup group] =>
            (groupStats.ContainsKey(group) ? groupStats[group] : stat()).Value;
    }
}