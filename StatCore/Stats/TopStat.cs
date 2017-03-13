using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCore.Stats
{
    public class TopStat<T> : IStat<T, IEnumerable<T>>
    {
        private readonly int maxSize;
        private readonly ConcurrentSortedSet<T> values;
        public TopStat(int maxSize, Func<T, T, bool> comparer)
        {
            this.maxSize = maxSize;
            values = new ConcurrentSortedSet<T>(comparer);
        }
        public void Add(T item)
        {
            values.Add(item);
        }

        public void Delete(T item)
        {
            //TODO: is it neccessary check?
            if (values.Contains(item))
                values.Remove(item);
        }

        public IEnumerable<T> Value => values.TakeLast(maxSize);
        public bool IsEmpty => values.Count == 0;
    }
}
