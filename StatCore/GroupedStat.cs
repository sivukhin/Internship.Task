using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using StatCore.Stats;

namespace StatCore
{
    public class OrderedGroupedStat<TIn, TOut, TGroup> : GroupedStat<TIn, TOut, TGroup> where TOut:IComparable
    {
        private ConcurrentSortedSet<Tuple<TOut, TGroup>> sortedGroups;
        public OrderedGroupedStat(Func<TIn, TGroup> grouper, Func<IStat<TIn, TOut>> stat) : base(grouper, stat)
        {
            sortedGroups = new ConcurrentSortedSet<Tuple<TOut, TGroup>>();
        }

        public override void Add(TIn item)
        {
            var oldStat = GetGroup(item);
            var oldValue = Tuple.Create(oldStat.Value, grouper(item));
            if (sortedGroups.Contains(oldValue))
                sortedGroups.Remove(oldValue);

            base.Add(item);
        }
    }
    public class GroupedStat<TIn, TOut, TGroup> : IStatStorage<TIn>
    {
        protected readonly Func<TIn, TGroup> grouper;
        protected readonly Func<IStat<TIn, TOut>> stat;
        protected ConcurrentDictionary<TGroup, IStat<TIn, TOut>> groupStats;

        public GroupedStat(Func<TIn, TGroup> grouper, Func<IStat<TIn, TOut>> stat)
        {
            this.grouper = grouper;
            this.stat = stat;
            groupStats = new ConcurrentDictionary<TGroup, IStat<TIn, TOut>>();
        }

        protected IStat<TIn, TOut> GetGroup(TIn item)
        {
            if (groupStats.ContainsKey(grouper(item)))
                return groupStats[grouper(item)];
            return groupStats[grouper(item)] = stat();
        }

        public virtual void Add(TIn item)
        {
            lock (groupStats)
                GetGroup(item).Add(item);
        }

        public virtual void Delete(TIn item)
        {
            lock (groupStats)
                GetGroup(item).Delete(item);
        }

        public TOut this[TGroup group]
        {
            get
            {
                lock (groupStats)
                {
                    return (groupStats.ContainsKey(group) ? groupStats[group] : stat()).Value;
                }
            }
        }
    }
}