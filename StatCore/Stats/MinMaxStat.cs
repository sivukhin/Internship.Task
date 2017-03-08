using System;
using System.Collections.Concurrent;

namespace StatCore.Stats
{
    public class MinMaxStat<TTarget, TResult> : IStat<TTarget, Tuple<TResult, TResult>>
    {
        private readonly Tuple<TResult, TResult> defaultValue = Tuple.Create(default(TResult), default(TResult));
        private readonly ConcurrentDictionary<TResult, int> resultCounter;
        private readonly ConcurrentSortedSet<TResult> resultSet;
        private readonly Func<TTarget, TResult> selector;

        public Tuple<TResult, TResult> Value { get; private set; }
        public bool IsEmpty => resultCounter.IsEmpty;

        public MinMaxStat(Func<TTarget, TResult> selector)
        {
            this.selector = selector;
            resultCounter = new ConcurrentDictionary<TResult, int>();
            resultSet = new ConcurrentSortedSet<TResult>();
            UpdateValue();
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
            UpdateValue();
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
            UpdateValue();
        }

        private void UpdateValue()
        {
            Value = resultSet.Count == 0 ? defaultValue : Tuple.Create(resultSet.Min, resultSet.Max);
        }
    }
}