using StatCore.Stats;

namespace StatCore
{
    public class CounterStat<TTarget> : IStat<TTarget, int>
    {
        public CounterStat(int initialValue = 0)
        {
            Value = initialValue;
        }
        public void Add(TTarget item)
        {
            Value++;
        }
        public void Delete(TTarget item)
        {
            Value--;
        }
        public int Value { get; private set; }
        public bool IsEmpty => Value == 0;
    }
}