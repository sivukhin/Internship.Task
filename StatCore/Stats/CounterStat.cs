namespace StatCore.Stats
{
    public class CounterStat<TTarget> : IStat<TTarget, int>
    {
        private readonly object counterLock = new object();
        public CounterStat(int initialValue = 0)
        {
            Value = initialValue;
        }
        public void Add(TTarget item)
        {
            lock (counterLock)
                Value++;
        }
        public void Delete(TTarget item)
        {
            lock (counterLock)
                Value--;
        }
        public int Value { get; private set; }
        public bool IsEmpty => Value == 0;
    }
}