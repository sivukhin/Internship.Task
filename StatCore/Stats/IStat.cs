namespace StatCore.Stats
{
    public interface IStat<in TTarget, out TResult>
    {
        void Add(TTarget item);
        void Delete(TTarget item);
        TResult Value { get; }
        bool IsEmpty { get; }
    }
}