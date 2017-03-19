using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StatCore.Stats;

namespace StatCore
{
    public class Report<T, TFeature> : IStat<T, IEnumerable<T>> where TFeature:IComparable
    {
        private readonly int maxSize;
        private readonly Func<T, TFeature> featureSelector;
        private readonly ConcurrentDictionary<T, TFeature> features;
        private readonly ConcurrentSortedSet<Tuple<TFeature, T>> itemSet;
        private readonly object reportLock = new object();

        public Report(int maxSize, Func<T, TFeature> featureSelector, IComparer<T> comparer)
        {
            this.maxSize = maxSize;
            this.featureSelector = featureSelector;
            features = new ConcurrentDictionary<T, TFeature>();
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

        public Report(Func<T, TFeature> featureSelector, Func<T, T, int> comparer) :
            this(-1, featureSelector, comparer)
        {
        }

        public Report(Func<T, TFeature> featureSelector, Func<T, T, bool> lessComparer) :
            this(-1, featureSelector, lessComparer)
        {
        }

        private void TryUpdate(T item)
        {
            lock (reportLock)
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
        }

        public void Add(T item)
        {
            TryUpdate(item);
        }

        public void Delete(T item)
        {
            TryUpdate(item);
        }

        public IEnumerable<T> Value
        {
            get
            {
                if (maxSize != -1)
                    return itemSet.TakeLast(maxSize).Select(pair => pair.Item2);
                return itemSet.TakeFirst(itemSet.Count).Select(pair => pair.Item2);
            }
        }

        public bool IsEmpty => itemSet.Count == 0;
    }
}
