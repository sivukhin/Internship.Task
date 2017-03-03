using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StatCore.Stats
{
    public class PopularStat<TTarget, TResult> : IStat<TTarget, IEnumerable<TResult>> where TResult : IComparable
    {
        private ConcurrentSortedSet<Tuple<int, TResult>> countSet;
        private ConcurrentDictionary<TResult, int> resultCount;
        private Func<TTarget, TResult> selector;
        private int MaxSize { get; set; }
        public PopularStat(int maxSize, Func<TTarget, TResult> selector)
        {
            countSet = new ConcurrentSortedSet<Tuple<int, TResult>>();
            resultCount = new ConcurrentDictionary<TResult, int>();
            MaxSize = maxSize;
            this.selector = selector;
        }
        public void Add(TTarget item)
        {
            var value = selector(item);
            if (!resultCount.ContainsKey(value))
            {
                resultCount[value] = 1;
                countSet.Add(Tuple.Create(1, value));
            }
            else
            {
                var oldCount = resultCount[value];
                countSet.Remove(Tuple.Create(oldCount, value));
                countSet.Add(Tuple.Create(oldCount + 1, value));
                resultCount[value]++;
            }
        }

        public void Delete(TTarget item)
        {
            var value = selector(item);
            if (!resultCount.ContainsKey(value))
                return;
            var oldCount = resultCount[value];
            countSet.Remove(Tuple.Create(oldCount, value));
            resultCount[value]--;
            if (oldCount > 1)
                countSet.Add(Tuple.Create(oldCount - 1, value));
            else
                resultCount.TryRemove(value, out oldCount);
        }

        public IEnumerable<TResult> Value => countSet.TakeLast(MaxSize).Select(pair => pair.Item2);
        public bool IsEmpty => countSet.Count == 0;
    }
}