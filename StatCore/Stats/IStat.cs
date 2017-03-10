using System;

namespace StatCore.Stats
{
    public interface IStatStorage<in TIn>
    {
        void Add(TIn item);
        void Delete(TIn item);
    }

    public interface IStatValue<out TOut>
    {
        TOut Value { get; }
        bool IsEmpty { get; }
    }

    public interface IStat<in TIn, out TOut> : IStatStorage<TIn>, IStatValue<TOut>
    {
        
    }

    public interface IConnectableStat<in TIn, TOut>
    {
        void Add(TIn item);
        void Delete(TIn item);

        event EventHandler<TOut> Added;
        event EventHandler<TOut> Deleted;
    }
}