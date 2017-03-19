using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public static class ConcurrentExtensions
    {
        public static bool Remove<T, U>(this ConcurrentDictionary<T, U> dictionary, T value)
        {
            U returned;
            return dictionary.TryRemove(value, out returned);
        }
    }
    public class DataSplitter<TBase, TIn, TSplit, TOut> : IConnectableStat<TBase, TOut>
    {
        private readonly IConnectableStat<TBase, TIn> baseStat;
        private readonly Func<TIn, TSplit> selector;
        private readonly Func<DataIdentity<TIn>, IStat<TIn, TOut>> statFactory;
        private readonly ConcurrentDictionary<TSplit, IStat<TIn, TOut>> groupValues;
        private readonly object splitterLock = new object();

        public DataSplitter(
            IConnectableStat<TBase, TIn> baseStat,
            Func<TIn, TSplit> selector,
            Func<DataIdentity<TIn>, IStat<TIn, TOut>> statFactory)
        {
            groupValues = new ConcurrentDictionary<TSplit, IStat<TIn, TOut>>();
            this.baseStat = baseStat;
            this.selector = selector;
            this.statFactory = statFactory;
            baseStat.Added += (_, item) =>
            {
                lock (splitterLock)
                    AddInGroup(GetGroup(item), item);
            };
            baseStat.Deleted += (_, item) =>
            {
                lock (splitterLock)
                {
                    while (!ExistGroup(item))
                        Monitor.Wait(splitterLock);
                    DeleteFromGroup(GetGroup(item), item);
                    if (GetGroup(item).IsEmpty)
                        groupValues.Remove(selector(item));
                }
            };
        }

        public void Add(TBase item)
        {
            baseStat.Add(item);
        }

        public void Delete(TBase item)
        {
            baseStat.Delete(item);
        }

        private bool ExistGroup(TIn item)
        {
            return groupValues.ContainsKey(selector(item));
        }

        private IStat<TIn, TOut> GetGroup(TIn item)
        {
            if (!ExistGroup(item))
            {
                var newGroup = groupValues[selector(item)] = statFactory(new DataIdentity<TIn>());
                Monitor.Pulse(splitterLock);
                return newGroup;
            }
            return groupValues[selector(item)];
        }

        private void AddInGroup(IStat<TIn, TOut> groupStat, TIn item)
        {
            if (!groupStat.IsEmpty)
                OnDeleted(groupStat.Value);
            groupStat.Add(item);
            OnAdded(groupStat.Value);
        }

        private void DeleteFromGroup(IStat<TIn, TOut> groupStat, TIn item)
        {
            OnDeleted(groupStat.Value);
            groupStat.Delete(item);
            if (!groupStat.IsEmpty)
                OnAdded(groupStat.Value);
        }

        public event EventHandler<TOut> Added;
        public event EventHandler<TOut> Deleted;

        protected virtual void OnAdded(TOut e)
        {
            Added?.Invoke(this, e);
        }

        protected virtual void OnDeleted(TOut e)
        {
            Deleted?.Invoke(this, e);
        }
    }
}
