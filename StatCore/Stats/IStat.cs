namespace StatCore.Stats
{
    public interface IStatStorage<in TTarget>
    {
        void Add(TTarget item);
        void Delete(TTarget item);
    }

    public interface IStatValue<out TResult>
    {
        TResult Value { get; }
        bool IsEmpty { get; }
    }

    public interface IStat<in TTarget, out TResult> : IStatStorage<TTarget>, IStatValue<TResult>
    {
        
    }
}