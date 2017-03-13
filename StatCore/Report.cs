using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatCore.Stats;

namespace StatCore
{
    public class Report<T> : IStat<T, IEnumerable<T>>
    {
        private readonly int maxSize;
        private readonly IComparer<T> comparer;
        protected readonly ConcurrentSortedSet<T> itemSet;

        public Report(int maxSize, IComparer<T> comparer)
        {
            this.maxSize = maxSize;
            this.comparer = comparer;
            this.itemSet = new ConcurrentSortedSet<T>();
        }

        public Report(int maxSize, Func<T, T, int> comparer) : this(maxSize, Comparer<T>.Create((x, y) => comparer(x, y)))
        {
        }

        public Report(int maxSize, Func<T, T, bool> lessComparer) : this(maxSize, Comparer<T>.Create((x, y) =>
        {
            if (lessComparer(x, y))
                return -1;
            return lessComparer(y, x) ? 1 : 0;
        }))
        {
        }

        public virtual void Add(T item)
        {
            itemSet.Add(item);
            if (itemSet.Count > maxSize)
                itemSet.Remove(itemSet.Max);
        }

        public virtual void Delete(T item)
        {

        }

        public IEnumerable<T> Value => itemSet.TakeFirst(maxSize);
        public bool IsEmpty => itemSet.Count == 0;
    }

    public class FullReport<T> : Report<T>
    {
        public FullReport(int maxSize, IComparer<T> comparer) : base(maxSize, comparer)
        {
        }

        public FullReport(int maxSize, Func<T, T, int> comparer) : base(maxSize, comparer)
        {
        }

        public FullReport(int maxSize, Func<T, T, bool> lessComparer) : base(maxSize, lessComparer)
        {
        }

        public override void Add(T item)
        {
            itemSet.Add(item);
        }

        public override void Delete(T item)
        {
            if (itemSet.Contains(item))
                itemSet.Remove(item);
        }
    }

}
