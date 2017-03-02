using System;
using System.Collections.Concurrent;

namespace StatCore.Stats
{
    public class MaxStat<TTarget, TResult> : IStat<TTarget, TResult>
    {
        private readonly Func<TTarget, TResult> selector;
        private readonly ConcurrentDictionary<TResult, int> resultCounter;
        private readonly ConcurrentSortedSet<TResult> resultSet;
        public MaxStat(Func<TTarget, TResult> selector)
        {
            this.selector = selector;
            resultCounter = new ConcurrentDictionary<TResult, int>();
            resultSet = new ConcurrentSortedSet<TResult>();
        }
        public void Add(TTarget item)
        {
            var result = selector(item);
            if (!resultCounter.ContainsKey(result))
            {
                resultCounter[result] = 1;
                resultSet.Add(result);
            }
            else
                resultCounter[result] += 1;
            Value = resultSet.Max;
        }

        public void Delete(TTarget item)
        {
            var result = selector(item);
            if (!resultCounter.ContainsKey(result))
                return;
            resultCounter[result]--;
            if (resultCounter[result] != 0)
                return;

            int x;
            resultCounter.TryRemove(result, out x);
            resultSet.Remove(result);
            Value = resultSet.Count == 0 ? default(TResult) : resultSet.Max;
        }

        public TResult Value { get; private set; }
        public bool IsEmpty => resultCounter.IsEmpty;
    }
}