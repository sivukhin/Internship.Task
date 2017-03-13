using System;
using System.Collections.Generic;
using System.Linq;

namespace StatCore
{
    public class ConcurrentSortedSet<T>
    {
        private readonly object setLock;
        private readonly SortedSet<T> set;

        public ConcurrentSortedSet()
        {
            set = new SortedSet<T>();
            setLock = new object();
        }

        public ConcurrentSortedSet(IComparer<T> comparer)
        {
            set = new SortedSet<T>(comparer);
            setLock = new object();
        }

        public ConcurrentSortedSet(Func<T, T, int> comparer) : this(Comparer<T>.Create((x, y) => comparer(x, y)))
        {
        }

        public ConcurrentSortedSet(Func<T, T, bool> lessComparer) : this(Comparer<T>.Create((x, y) =>
        {
            if (lessComparer(x, y))
                return -1;
            return lessComparer(y, x) ? 1 : 0;
        }))
        {
        }

        public void Add(T value)
        {
            lock (setLock)
            {
                set.Add(value);
            }
        }

        public T Max
        {
            get
            {
                lock (setLock)
                    return set.Max;
            }
        }

        public T Min
        {
            get
            {
                lock (setLock)
                    return set.Min;
            }
        }

        public int Count
        {
            get
            {
                lock (setLock)
                {
                    return set.Count;
                }
            }
        }

        public bool Contains(T value)
        {
            lock (setLock)
            {
                return set.Contains(value);
            }
        }

        public void Remove(T value)
        {
            lock (setLock)
            {
                set.Remove(value);
            }
        }

        public IEnumerable<T> TakeLast(int count)
        {
            lock (setLock)
            {
                return set.Reverse().Take(count).ToList();
            }
        }

        public IEnumerable<T> TakeFirst(int count)
        {
            lock (setLock)
            {
                return set.Take(count).ToList();
            }
        }
    }
}