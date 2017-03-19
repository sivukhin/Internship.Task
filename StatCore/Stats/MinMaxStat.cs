using System;
using System.Collections.Concurrent;
using System.Threading;

namespace StatCore.Stats
{
    public class MinMaxStat<TIn, TOut> : IStat<TIn, Tuple<TOut, TOut>>
    {
        private readonly Tuple<TOut, TOut> defaultValue = Tuple.Create(default(TOut), default(TOut));
        private readonly ConcurrentDictionary<TOut, int> resultCounter;
        private readonly ConcurrentSortedSet<TOut> resultSet;
        private readonly Func<TIn, TOut> selector;
        private readonly object minMaxLock = new object();

        public Tuple<TOut, TOut> Value { get; private set; }
        public bool IsEmpty => resultCounter.IsEmpty;

        public MinMaxStat(Func<TIn, TOut> selector)
        {
            this.selector = selector;
            resultCounter = new ConcurrentDictionary<TOut, int>();
            resultSet = new ConcurrentSortedSet<TOut>();
            UpdateValue();
        }

        public void Add(TIn item)
        {
            var result = selector(item);
            lock (minMaxLock)
            {
                if (!resultCounter.ContainsKey(result))
                {
                    Monitor.Pulse(minMaxLock);
                    resultCounter[result] = 1;
                    resultSet.Add(result);
                }
                else
                    resultCounter[result] += 1;
            }
            UpdateValue();
        }

        public void Delete(TIn item)
        {
            var result = selector(item);
            lock (minMaxLock)
            {
                while (!resultCounter.ContainsKey(result))
                    Monitor.Wait(minMaxLock);

                resultCounter[result]--;
                if (resultCounter[result] != 0)
                    return;

                int x;
                resultCounter.TryRemove(result, out x);
                resultSet.Remove(result);
            }
            UpdateValue();
        }

        private void UpdateValue()
        {
            lock (minMaxLock)
                Value = resultSet.Count == 0 ? defaultValue : Tuple.Create(resultSet.Min, resultSet.Max);
        }
    }
}