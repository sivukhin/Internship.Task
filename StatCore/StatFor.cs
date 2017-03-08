using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using StatCore.Stats;

namespace StatCore
{
    public static class StatFor<TTarget>
    {
        public static GroupStat<TGroup, TTarget> Group<TGroup>(Func<TTarget, TGroup> grouper)
        {
            return new GroupStat<TGroup, TTarget>(grouper);
        }

        public static IStat<TTarget, int> Count(int initialValue = 0)
        {
            return new CounterStat<TTarget>(initialValue);
        }

        public static IStat<TTarget, TResult> Max<TResult>(Func<TTarget, TResult> selector)
        {
            return new MinMaxStat<TTarget, TResult>(selector).Select(minMax => minMax.Item2);
        }

        public static IStat<TTarget, double> Average(Func<TTarget, double> initialStat)
        {
            return new AverageStat<TTarget>(initialStat);
        }

        public static IStat<TTarget, double> AverageByGroup<TGroup>(Func<TTarget, TGroup> grouper, Func<IStat<TTarget, double>> initialStat)
        {
            return new GroupedAverageStat<TTarget, TGroup>(grouper, initialStat);
        }

        public static IStat<TTarget, IEnumerable<TResult>> Popular<TResult>(int maxSize, Func<TTarget, TResult> selector)
            where TResult : IComparable
        {
            return new PopularStat<TTarget, TResult>(maxSize, selector);
        }
    }
}
