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
                {
                    return set.Max;
                }
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

        public void Remove(T value)
        {
            lock (setLock)
            {
                set.Remove(value);
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