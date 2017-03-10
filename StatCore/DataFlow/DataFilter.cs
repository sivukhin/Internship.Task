using System;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class DataFilter<TIn, TOut> : IConnectableStat<TIn, TOut>
    {
        private readonly IConnectableStat<TIn, TOut> baseStat;
        private readonly Func<TOut, bool> predicate;
        public DataFilter(IConnectableStat<TIn, TOut> baseStat, Func<TOut, bool> predicate)
        {
            this.baseStat = baseStat;
            this.predicate = predicate;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            baseStat.Added += (_, item) =>
            {
                if (predicate(item))
                    OnAdded(item);
            };
            baseStat.Deleted += (_, item) =>
            {
                if (predicate(item))
                    OnDeleted(item);
            };
        }

        public void Add(TIn item)
        {
            baseStat.Add(item);
        }

        public void Delete(TIn item)
        {
            baseStat.Delete(item);
        }

        public event EventHandler<TOut> Added;
        public event EventHandler<TOut> Deleted;

        protected virtual void OnAdded(TOut item)
        {
            Added?.Invoke(this, item);
        }

        protected virtual void OnDeleted(TOut item)
        {
            Deleted?.Invoke(this, item);
        }
    }
}