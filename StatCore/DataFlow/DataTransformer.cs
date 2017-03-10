using System;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class DataTransformer<TIn, TMid, TOut> : IConnectableStat<TIn, TOut>
    {
        private readonly IConnectableStat<TIn, TMid> baseStat;
        private readonly Func<TMid, TOut> selector;
        public DataTransformer(IConnectableStat<TIn, TMid> baseStat, Func<TMid, TOut> selector)
        {
            this.baseStat = baseStat;
            this.selector = selector;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            baseStat.Added += (_, item) => OnAdded(selector(item));
            baseStat.Deleted += (_, item) => OnDeleted(selector(item));
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

        protected virtual void OnDeleted(TOut e)
        {
            Deleted?.Invoke(this, e);
        }

        protected virtual void OnAdded(TOut e)
        {
            Added?.Invoke(this, e);
        }
    }
}