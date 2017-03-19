using System;
using System.Collections.Generic;
using System.Linq;
using StatCore.DataFlow;
using StatCore.Stats;

namespace StatCore
{
    public static class StatExtensions
    {
        public static IConnectableStat<TIn, TOut> Where<TIn, TOut>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, bool> predicate)
        {
            return new DataFilter<TIn, TOut>(connection, predicate);
        }

        public static IConnectableStat<TIn, TOut> Select<TIn, TMid, TOut>(this IConnectableStat<TIn, TMid> connection,
            Func<TMid, TOut> selector)
        {
            return new DataTransformer<TIn, TMid, TOut>(connection, selector);
        }

        public static IStat<TIn, TOut> Select<TIn, TMid, TOut>(this IStat<TIn, TMid> stat, Func<TMid, TOut> selector)
        {
            return new StatTransformer<TIn, TMid, TOut>(stat, selector);
        }

        public static IStat<TIn, TStat> ConnectTo<TIn, TOut, TStat>(this IConnectableStat<TIn, TOut> connection,
            IStat<TOut, TStat> stat)
        {
            return new StatConnecter<TIn, TOut, TStat>(connection, stat);
        }

        public static IConnectableStat<TIn, TOut> ConnectTo<TIn, TMid, TOut>(this IConnectableStat<TIn, TMid> firstConnection,
            IConnectableStat<TMid, TOut> secondConnection)
        {
            return new DataConnecter<TIn, TMid, TOut>(firstConnection, secondConnection);
        }

        public static IStat<TIn, TStat> Max<TIn, TOut, TStat>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, TStat> selector)
        {
            return connection.ConnectTo(new MinMaxStat<TOut, TStat>(selector).Select(minMax => minMax.Item2));
        }

        public static IStat<TIn, TOut> Max<TIn, TOut>(this IConnectableStat<TIn, TOut> connection)
        {
            return connection.ConnectTo(new MinMaxStat<TOut, TOut>(x => x).Select(minMax => minMax.Item2));
        }

        public static IStat<TIn, TStat> Min<TIn, TOut, TStat>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, TStat> selector)
        {
            return connection.ConnectTo(new MinMaxStat<TOut, TStat>(selector).Select(minMax => minMax.Item1));
        }

        public static IStat<TIn, TOut> Min<TIn, TOut>(this IConnectableStat<TIn, TOut> connection)
        {
            return connection.ConnectTo(new MinMaxStat<TOut, TOut>(x => x).Select(minMax => minMax.Item1));
        }

        public static IStat<TIn, int> Count<TIn, TOut>(this IConnectableStat<TIn, TOut> connection, int initValue = 0)
        {
            return connection.ConnectTo(new CounterStat<TOut>(initValue));
        }

        public static IStat<TIn, bool> Existence<TIn, TOut>(this IConnectableStat<TIn, TOut> connection)
        {
            return connection.Count().Select(count => count > 0);
        }

        public static IStat<TIn, int> Sum<TIn, TOut>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, int> selector)
        {
            return connection.ConnectTo(new SumStat<TOut>(selector));
        }

        public static IStat<TIn, int> Sum<TIn>(this IConnectableStat<TIn, int> connection)
        {
            return connection.ConnectTo(new SumStat<int>(x => x));
        }

        public static IStat<TIn, double> Average<TIn, TOut>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, double> selector)
        {
            return connection.ConnectTo(new AverageStat<TOut>(selector));
        }

        public static IStat<TIn, double> Average<TIn>(this IConnectableStat<TIn, double> connection)
        {
            return connection.ConnectTo(new AverageStat<double>(x => x));
        }

        public static IStat<TIn, double> Average<TIn>(this IConnectableStat<TIn, int> connection)
        {
            return connection.Select(x => (double)x).Average();
        }

        public static IStat<TIn, IEnumerable<TOut>> Popular<TIn, TOut>(
            this IConnectableStat<TIn, TOut> connection,
            int maxSize) where TOut : IComparable
        {
            return connection.ConnectTo(new PopularStat<TOut>(maxSize));
        }

        public static IStat<TIn, TOut> Favorite<TIn, TOut>(
            this IConnectableStat<TIn, TOut> connection) where TOut : IComparable
        {
            return connection.Popular(1).Select(seq => seq.SingleOrDefault());
        }

        public static IConnectableStat<TIn, TOut> Split<TIn, TMid, TOut, TSplit>(
            this IConnectableStat<TIn, TMid> connection,
            Func<TMid, TSplit> selector,
            Func<DataIdentity<TMid>, IStat<TMid, TOut>> statFactory)
        {
            return new DataSplitter<TIn, TMid, TSplit, TOut>(connection, selector, statFactory);
        }

        public static IStat<TIn, IEnumerable<TOut>> Top<TIn, TOut>(
            this IConnectableStat<TIn, TOut> connection,
            int maxSize,
            Func<TOut, TOut, bool> comparer)
        {
            return connection.ConnectTo(new TopStat<TOut>(maxSize, comparer));
        }

        public static IStat<TIn, IEnumerable<TOut>> Report<TIn, TOut, TFeature>(this IConnectableStat<TIn, TOut> connection,
            int maxSize,
            Func<TOut, TFeature> featureSelector,
            Func<TOut, TOut, bool> lessComparer) where TFeature : IComparable
        {
            return connection.ConnectTo(new Report<TOut, TFeature>(maxSize, featureSelector, lessComparer));
        }

        public static IStat<TIn, IEnumerable<TOut>> Report<TIn, TOut, TFeature>(this IConnectableStat<TIn, TOut> connection,
            int maxSize,
            Func<TOut, TFeature> featureSelector,
            Func<TOut, TOut, int> comparer) where TFeature : IComparable
        {
            return connection.ConnectTo(new Report<TOut, TFeature>(maxSize, featureSelector, comparer));
        }

        public static IStat<TIn, IEnumerable<TOut>> Report<TIn, TOut, TFeature>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, TFeature> featureSelector,
            Func<TOut, TOut, bool> lessComparer) where TFeature : IComparable
        {
            return connection.ConnectTo(new Report<TOut, TFeature>(featureSelector, lessComparer));
        }

        public static IStat<TIn, IEnumerable<TOut>> Report<TIn, TOut, TFeature>(this IConnectableStat<TIn, TOut> connection,
            Func<TOut, TFeature> featureSelector,
            Func<TOut, TOut, int> comparer) where TFeature : IComparable
        {
            return connection.ConnectTo(new Report<TOut, TFeature>(featureSelector, comparer));
        }
    }
}