using System;
using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class DataIdentity<T> : IConnectableStat<T, T>
    {
        public void Add(T item)
        {
            OnAdded(item);
        }

        public void Delete(T item)
        {
            OnDeleted(item);
        }

        public event EventHandler<T> Added;
        public event EventHandler<T> Deleted;

        protected virtual void OnAdded(T item)
        {
            Added?.Invoke(this, item);
        }

        protected virtual void OnDeleted(T item)
        {
            Deleted?.Invoke(this, item);
        }
    }
}