using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StatCore.Stats
{
    public class PopularStat<T> : IStat<T, IEnumerable<T>> where T : IComparable
    {
        private readonly ConcurrentSortedSet<Tuple<int, T>> countSet;
        private readonly ConcurrentDictionary<T, int> resultCount;
        private readonly object popularLock = new object();
        private int MaxSize { get; set; }
        public PopularStat(int maxSize)
        {
            countSet = new ConcurrentSortedSet<Tuple<int, T>>();
            resultCount = new ConcurrentDictionary<T, int>();
            MaxSize = maxSize;
        }
        public void Add(T item)
        {
            lock (popularLock)
            {
                if (!resultCount.ContainsKey(item))
                {
                    Monitor.Pulse(popularLock);
                    resultCount[item] = 1;
                    countSet.Add(Tuple.Create(1, item));
                }
                else
                {
                    var oldCount = resultCount[item];
                    countSet.Remove(Tuple.Create(oldCount, item));
                    countSet.Add(Tuple.Create(oldCount + 1, item));
                    resultCount[item]++;
                }
            }
        }

        public void Delete(T item)
        {
            lock (popularLock)
            {
                while (!resultCount.ContainsKey(item))
                    Monitor.Wait(popularLock);
                var oldCount = resultCount[item];
                countSet.Remove(Tuple.Create(oldCount, item));
                resultCount[item]--;
                if (oldCount > 1)
                    countSet.Add(Tuple.Create(oldCount - 1, item));
                else
                    resultCount.TryRemove(item, out oldCount);
            }
        }

        public IEnumerable<T> Value => countSet.TakeLast(MaxSize).Select(pair => pair.Item2);
        public bool IsEmpty => countSet.Count == 0;
    }
}