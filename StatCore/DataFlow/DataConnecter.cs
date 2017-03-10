using System;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class DataConnecter<TIn, TMid, TOut> : IConnectableStat<TIn, TOut>
    {
        private readonly IConnectableStat<TIn, TMid> firstConnection;
        private readonly IConnectableStat<TMid, TOut> secondConnection;

        public DataConnecter(IConnectableStat<TIn, TMid> firstConnection, 
            IConnectableStat<TMid, TOut> secondConnection)
        {
            this.firstConnection = firstConnection;
            this.secondConnection = secondConnection;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            firstConnection.Added += (_, item) => secondConnection.Add(item);
            firstConnection.Deleted += (_, item) => secondConnection.Add(item);

            secondConnection.Added += (_, item) => OnAdded(item);
            secondConnection.Deleted += (_, item) => OnDeleted(item);
        }

        public void Add(TIn item)
        {
            firstConnection.Add(item);
        }

        public void Delete(TIn item)
        {
            firstConnection.Delete(item);
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