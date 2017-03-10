using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class DataSplitter<TIn, TSplit, TOut> : IConnectableStat<TIn, TOut>
    {
        private readonly Func<TIn, TSplit> selector;
        private readonly Func<DataIdentity<TIn>, IStat<TIn, TOut>> statFactory;
        private readonly ConcurrentDictionary<TSplit, IStat<TIn, TOut>> groupValues;

        public DataSplitter(
            Func<TIn, TSplit> selector,
            Func<DataIdentity<TIn>, IStat<TIn, TOut>> statFactory)
        {
            groupValues = new ConcurrentDictionary<TSplit, IStat<TIn, TOut>>();
            this.selector = selector;
            this.statFactory = statFactory;
        }

        public void Add(TIn item)
        {
            AddInGroup(GetGroup(item), item);
        }

        public void Delete(TIn item)
        {
            if (!ExistGroup(item))
                return;
            DeleteFromGroup(GetGroup(item), item);
        }

        private bool ExistGroup(TIn item)
        {
            return groupValues.ContainsKey(selector(item));
        }

        private IStat<TIn, TOut> GetGroup(TIn item)
        {
            if (!ExistGroup(item))
                return groupValues[selector(item)] = statFactory(new DataIdentity<TIn>());
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
