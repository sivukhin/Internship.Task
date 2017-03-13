using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatCore.Stats;

namespace StatCore
{
    public class Report<T, TFeature> : IStat<T, IEnumerable<T>> where TFeature:IComparable
    {
        private readonly int maxSize;
        private readonly Func<T, TFeature> featureSelector;
        private readonly IComparer<T> comparer;
        private readonly Dictionary<T, TFeature> features;
        private readonly ConcurrentSortedSet<Tuple<TFeature, T>> itemSet;

        public Report(int maxSize, Func<T, TFeature> featureSelector, IComparer<T> comparer)
        {
            this.maxSize = maxSize;
            this.featureSelector = featureSelector;
            this.comparer = comparer;
            features = new Dictionary<T, TFeature>();
            itemSet = new ConcurrentSortedSet<Tuple<TFeature, T>>(Comparer<Tuple<TFeature, T>>.Create((x, y) =>
            {
                var firstComponent = x.Item1.CompareTo(y.Item1);
                return firstComponent != 0 ? firstComponent : comparer.Compare(x.Item2, y.Item2);
            }));
        }

        public Report(int maxSize, Func<T, TFeature> featureSelector, Func<T, T, int> comparer) : 
            this(maxSize, featureSelector, Comparer<T>.Create((x, y) => comparer(x, y)))
        {
            
        }

        public Report(int maxSize, Func<T, TFeature> featureSelector, Func<T, T, bool> lessComparer) :
            this(maxSize, featureSelector, Comparer<T>.Create((x, y) =>
            {
                if (lessComparer(x, y))
                    return -1;
                if (lessComparer(y, x))
                    return 1;
                return 0;
            }))
        {

        }

        private void TryUpdate(T item)
        {
            var newFeature = featureSelector(item);
            if (features.ContainsKey(item))
            {
                var oldFeature = features[item];
                if (oldFeature.Equals(newFeature))
                    return;
                itemSet.Remove(Tuple.Create(oldFeature, item));
            }
            features[item] = newFeature;
            itemSet.Add(Tuple.Create(newFeature, item));
        }

        public virtual void Add(T item)
        {
            TryUpdate(item);
        }

        public virtual void Delete(T item)
        {
            TryUpdate(item);
        }

        public IEnumerable<T> Value => itemSet.TakeLast(maxSize).Select(pair => pair.Item2);
        public bool IsEmpty => itemSet.Count == 0;
    }
}
