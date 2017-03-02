using System;
using System.Collections.Generic;
using StatCore.Stats;

namespace StatCore
{
    public class GroupStat<TTarget, TResult, TGroup>
    {
        private readonly Func<TTarget, TGroup> grouper;
        private readonly Func<IStat<TTarget, TResult>> initialStat;
        private Dictionary<TGroup, IStat<TTarget, TResult>> groupStats;

        public GroupStat(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, TResult>> initialStat)
        {
            this.grouper = grouper;
            this.initialStat = initialStat;
            groupStats = new Dictionary<TGroup, IStat<TTarget, TResult>>();
        }

        public IStat<TTarget, TResult> GetGroup(TTarget item)
        {
            if (groupStats.ContainsKey(grouper(item)))
                return groupStats[grouper(item)];
            return groupStats[grouper(item)] = initialStat();
        }

        public void Add(TTarget item)
        {
            GetGroup(item).Add(item);
        }

        public void Delete(TTarget item)
        {
            GetGroup(item).Delete(item);
        }

        public TResult this[TGroup group] =>
            (groupStats.ContainsKey(group) ? groupStats[group] : initialStat()).Value;
    }

    public class GroupStat<TGroup, TTarget>
    {
        private Func<TTarget, TGroup> grouper;

        public GroupStat(Func<TTarget, TGroup> grouper)
        {
            this.grouper = grouper;
        }

        public GroupStat<TTarget, TResult, TGroup> Calc<TResult>(Func<IStat<TTarget, TResult>> stat)
        {
            return new GroupStat<TTarget, TResult, TGroup>(grouper, stat);
        }
        public GroupStat<TTarget, int, TGroup> Count(int initialValue = 0)
        {
            return Calc(() => new CounterStat<TTarget>(initialValue));
        }

        public GroupStat<TTarget, TResult, TGroup> Max<TResult>(Func<TTarget, TResult> selector)
        {
            return Calc(() => new MaxStat<TTarget, TResult>(selector));
        }

        public GroupStat<TTarget, TResult, TGroup> MaxByGroup<TResult, TMaxGroup>(
            Func<TTarget, TMaxGroup> maxGrouper, Func<IStat<TTarget, TResult>> initialStat)
        {
            return Calc(() => new GroupedMaxStat<TTarget, TResult, TMaxGroup>(maxGrouper, initialStat));
        }

        public GroupStat<TTarget, double, TGroup> Average(Func<TTarget, double> initialStat)
        {
            return Calc(() => new AverageStat<TTarget>(initialStat));
        }

        public GroupStat<TTarget, double, TGroup> AverageByGroup<TAvgGroup>(
            Func<TTarget, TAvgGroup> averageGrouper, Func<IStat<TTarget, double>> initialStat)
        {
            return Calc(() => new GroupedAverageStat<TTarget, TAvgGroup>(averageGrouper, initialStat));
        }

        public GroupStat<TTarget, IEnumerable<TResult>, TGroup> Popular<TResult>(
            int maxSize, Func<TTarget, TResult> selector) where TResult : IComparable
        {
            return Calc(() => new PopularStat<TTarget, TResult>(maxSize, selector));
        }

    }
}