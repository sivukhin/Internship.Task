using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCore
{
    public interface IStat<in TTarget, out TResult>
    {
        void Add(TTarget item);
        void Delete(TTarget item);
        TResult Value { get; }
        bool IsEmpty { get; }
    }
    public static class StatFor<TTarget>
    {
        public static GroupStat<TGroup, TTarget> Group<TGroup>(Func<TTarget, TGroup> grouper)
        {
            return new GroupStat<TGroup, TTarget>(grouper);
        }

        public static Func<IStat<TTarget, int>> Count(int initialValue = 0)
        {
            return () => new CounterStat<TTarget>(initialValue);
        }

        public static Func<IStat<TTarget, double>> FloatCount(double initialValue = 0)
        {
            return () => new FloatCounterStat<TTarget>(initialValue);
        }

        public static Func<IStat<TTarget, TResult>> Max<TResult>(Func<TTarget, TResult> selector)
        {
            return () => new MaxStat<TTarget, TResult>(selector);
        }

        public static Func<IStat<TTarget, double>> Average(Func<IStat<TTarget, double>> initialStat)
        {
            return () => new AverageStat<TTarget>(initialStat);
        }

        public static Func<IStat<TTarget, double>> AverageByGroup<TGroup>(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, double>> initialStat)
        {
            return () => new GroupedAverageStat<TTarget, TGroup>(grouper, initialStat);
        }

        public static Func<IStat<TTarget, IEnumerable<TResult>>> Popular<TResult>(int maxSize, Func<TTarget, TResult> selector)
            where TResult : IComparable
        {
            return () => new PopularStat<TTarget, TResult>(maxSize, selector);
        }
    }

    public class GroupStat<TGroup, TTarget>
    {
        private Func<TTarget, TGroup> grouper;

        public GroupStat(Func<TTarget, TGroup> grouper)
        {
            this.grouper = grouper;
        }

        public GroupStat<TGroup, TTarget, TResult> Calc<TResult>(Func<IStat<TTarget, TResult>> stat)
        {
            return new GroupStat<TGroup, TTarget, TResult>(grouper, stat);
        }
        public GroupStat<TGroup, TTarget, int> Count(int initialValue = 0)
        {
            return Calc(() => new CounterStat<TTarget>(initialValue));
        }

        public GroupStat<TGroup, TTarget, TResult> Max<TResult>(Func<TTarget, TResult> selector)
        {
            return Calc(() => new MaxStat<TTarget, TResult>(selector));
        }

        public GroupStat<TGroup, TTarget, double> Average(Func<IStat<TTarget, double>> initialStat)
        {
            return Calc(() => new AverageStat<TTarget>(initialStat));
        }

        public GroupStat<TGroup, TTarget, double> AverageByGroup<TAvgGroup>(
            Func<TTarget, TAvgGroup> averageGrouper, Func<IStat<TTarget, double>> initialStat)
        {
            return Calc(() => new GroupedAverageStat<TTarget, TAvgGroup>(averageGrouper, initialStat));
        }

        public GroupStat<TGroup, TTarget, IEnumerable<TResult>> Average<TResult>(
            int maxSize, Func<TTarget, TResult> selector) where TResult : IComparable
        {
            return Calc(() => new PopularStat<TTarget, TResult>(maxSize, selector));
        }
    }

    public class GroupStat<TGroup, TTarget, TResult>
    {
        private readonly Func<TTarget, TGroup> grouper;
        private readonly Func<IStat<TTarget, TResult>> initialStat;
        private Dictionary<TGroup, IStat<TTarget, TResult>> groupStats;

        public GroupStat(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, TResult>> initialStat)
        {
            this.grouper = grouper;
            this.initialStat = initialStat;
            groupStats = new Dictionary<TGroup, IStat<TTarget, TResult>>();
        }

        public IStat<TTarget, TResult> GetGroup(TTarget item)
        {
            if (groupStats.ContainsKey(grouper(item)))
                return groupStats[grouper(item)];
            return groupStats[grouper(item)] = initialStat();
        }

        public void Add(TTarget item)
        {
            GetGroup(item).Add(item);
        }

        public void Delete(TTarget item)
        {
            GetGroup(item).Delete(item);
        }

        public TResult this[TGroup group] =>
            (groupStats.ContainsKey(group) ? groupStats[group] : initialStat()).Value;
    }

    public class ConcurrentSortedSet<T>
    {
        private object setLock;
        private SortedSet<T> set;

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

    public class CounterStat<TTarget> : IStat<TTarget, int>
    {
        public CounterStat(int initialValue = 0)
        {
            Value = initialValue;
        }
        public void Add(TTarget item)
        {
            Value++;
        }
        public void Delete(TTarget item)
        {
            Value--;
        }
        public int Value { get; private set; }
        public bool IsEmpty => Value == 0;
    }

    public class FloatCounterStat<TTarget> : IStat<TTarget, double>
    {
        public FloatCounterStat(double initialValue = 0)
        {
            Value = initialValue;
        }
        public void Add(TTarget item)
        {
            Value++;
        }
        public void Delete(TTarget item)
        {
            Value--;
        }
        public double Value { get; private set; }
        public bool IsEmpty => Value == 0;
    }

    public class AverageValue
    {
        private int Count { get; set; }
        private double Sum { get; set; }

        public double Value
        {
            get
            {
                if (Count == 0)
                    return 0;
                return Sum / Count;
            }
        }

        public static AverageValue operator +(AverageValue average, double value)
        {
            return new AverageValue { Count = average.Count + 1, Sum = average.Sum + value };
        }

        public static AverageValue operator -(AverageValue average, double value)
        {
            return new AverageValue { Count = average.Count - 1, Sum = average.Sum - value };
        }
    }

    public class GroupedAverageStat<TTarget, TGroup> : BaseAverageStat<TTarget>
    {
        private readonly ConcurrentDictionary<TGroup, IStat<TTarget, double>> groupValues;
        private readonly Func<TTarget, TGroup> grouper;
        public GroupedAverageStat(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, double>> initialStat)
            : base(initialStat)
        {
            this.grouper = grouper;
            groupValues = new ConcurrentDictionary<TGroup, IStat<TTarget, double>>();
        }

        public override bool IsEmpty => groupValues.IsEmpty;

        protected override bool ExistGroup(TTarget item)
        {
            return groupValues.ContainsKey(grouper(item));
        }

        protected override IStat<TTarget, double> GetGroup(TTarget item)
        {
            if (!ExistGroup(item))
                return groupValues[grouper(item)] = initialStat();
            return groupValues[grouper(item)];
        }
    }

    public class AverageStat<TTarget> : BaseAverageStat<TTarget>
    {
        private IStat<TTarget, double> Stat { get; set; }
        public AverageStat(Func<IStat<TTarget, double>> initialStat)
            : base(initialStat)
        {
            Stat = initialStat();
        }

        public override bool IsEmpty => Stat.IsEmpty;

        protected override bool ExistGroup(TTarget item)
        {
            return true;
        }

        protected override IStat<TTarget, double> GetGroup(TTarget item)
        {
            return Stat;
        }
    }

    public abstract class BaseAverageStat<TTarget> : IStat<TTarget, double>
    {
        protected AverageValue value;
        public double Value => value.Value;
        public abstract bool IsEmpty { get; }

        protected readonly Func<IStat<TTarget, double>> initialStat;

        protected BaseAverageStat(Func<IStat<TTarget, double>> initialStat)
        {
            this.initialStat = initialStat;
            value = new AverageValue();
        }

        protected abstract bool ExistGroup(TTarget item);

        protected abstract IStat<TTarget, double> GetGroup(TTarget item);

        private void AddInGroup(IStat<TTarget, double> groupStat, TTarget item)
        {
            if (!groupStat.IsEmpty)
                value -= groupStat.Value;
            groupStat.Add(item);
            value += groupStat.Value;
        }

        private void DeleteFromGroup(IStat<TTarget, double> groupStat, TTarget item)
        {
            value -= groupStat.Value;
            groupStat.Delete(item);
            if (!groupStat.IsEmpty)
                value += groupStat.Value;
        }

        public void Add(TTarget item)
        {
            AddInGroup(GetGroup(item), item);
        }

        public void Delete(TTarget item)
        {
            if (!ExistGroup(item))
                return;
            DeleteFromGroup(GetGroup(item), item);
        }
    }

    public class MaxStat<TTarget, TResult> : IStat<TTarget, TResult>
    {
        private readonly Func<TTarget, TResult> selector;
        private readonly ConcurrentDictionary<TResult, int> resultCounter;
        private readonly ConcurrentSortedSet<TResult> resultSet;
        public MaxStat(Func<TTarget, TResult> selector)
        {
            this.selector = selector;
            resultCounter = new ConcurrentDictionary<TResult, int>();
            resultSet = new ConcurrentSortedSet<TResult>();
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
            Value = resultSet.Max;
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
            Value = resultSet.Count == 0 ? default(TResult) : resultSet.Max;
        }

        public TResult Value { get; private set; }
        public bool IsEmpty => resultCounter.IsEmpty;
    }

    public class PopularStat<TTarget, TResult> : IStat<TTarget, IEnumerable<TResult>> where TResult : IComparable
    {
        private ConcurrentSortedSet<Tuple<int, TResult>> countSet;
        private ConcurrentDictionary<TResult, int> resultCount;
        private Func<TTarget, TResult> selector;
        public int MaxSize { get; set; }
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
            if (oldCount > 0)
                countSet.Add(Tuple.Create(oldCount - 1, value));
            else
                resultCount.TryRemove(value, out oldCount);
        }

        public IEnumerable<TResult> Value => countSet.TakeFirst(MaxSize).Select(pair => pair.Item2);
        public bool IsEmpty => countSet.Count == 0;
    }
}
