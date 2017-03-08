using System;
using System.Collections.Concurrent;

namespace StatCore.Stats
{
    public class BaseGroupedStat<TTarget, TResult, TGroup> : IStat<TTarget, TResult>
    {
        private readonly IStat<TResult, TResult> baseStat;
        private readonly Func<TTarget, TGroup> grouper;
        private readonly ConcurrentDictionary<TGroup, IStat<TTarget, TResult>> groupValues;
        private readonly Func<IStat<TTarget, TResult>> initialStat;
        public TResult Value => baseStat.Value;
        public bool IsEmpty => baseStat.IsEmpty;

        protected BaseGroupedStat(
            Func<TTarget, TGroup> grouper,
            Func<IStat<TTarget, TResult>> initialStat,
            IStat<TResult, TResult> baseStat)
        {
            groupValues = new ConcurrentDictionary<TGroup, IStat<TTarget, TResult>>();
            this.grouper = grouper;
            this.initialStat = initialStat;
            this.baseStat = baseStat;
        }

        public void Add(TTarget item)
        {
            AddInGroup(GetGroup(item), item);
        }

        public void Delete(TTarget item)
        {
            if (!ExistGroup(item))
                return;
            DeleteFromGroup(GetGroup(item), item);
        }

        private bool ExistGroup(TTarget item)
        {
            return groupValues.ContainsKey(grouper(item));
        }

        private IStat<TTarget, TResult> GetGroup(TTarget item)
        {
            if (!ExistGroup(item))
                return groupValues[grouper(item)] = initialStat();
            return groupValues[grouper(item)];
        }

        private void AddInGroup(IStat<TTarget, TResult> groupStat, TTarget item)
        {
            if (!groupStat.IsEmpty)
                baseStat.Delete(groupStat.Value);
            groupStat.Add(item);
            baseStat.Add(groupStat.Value);
        }

        private void DeleteFromGroup(IStat<TTarget, TResult> groupStat, TTarget item)
        {
            baseStat.Delete(groupStat.Value);
            groupStat.Delete(item);
            if (!groupStat.IsEmpty)
                baseStat.Add(groupStat.Value);
        }
    }

    public class GroupedAverageStat<TTarget, TGroup> : BaseGroupedStat<TTarget, double, TGroup>
    {
        public GroupedAverageStat(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, double>> initialStat) :
            base(grouper, initialStat, new AverageStat<double>(i => i))
        {
        }
    }

    public class GroupedMaxStat<TTarget, TResult, TGroup> : BaseGroupedStat<TTarget, TResult, TGroup>
    {
        public GroupedMaxStat(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, TResult>> initialStat) :
            base(grouper, initialStat, new MinMaxStat<TResult, TResult>(i => i).Select(minMax => minMax.Item2))
        {
        }
    }
}