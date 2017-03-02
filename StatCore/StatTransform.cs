using System;
using StatCore.Stats;

namespace StatCore
{
    public class StatTransform<TTarget, TResult, TInner> : IStat<TTarget, TResult>
    {
        private readonly IStat<TTarget, TInner> stat;
        private readonly Func<TInner, TResult> selector;
        public StatTransform(IStat<TTarget, TInner> stat, Func<TInner, TResult> selector)
        {
            this.stat = stat;
            this.selector = selector;
        }

        public void Add(TTarget item)
        {
            stat.Add(item);
        }

        public void Delete(TTarget item)
        {
            stat.Delete(item);
        }

        public TResult Value => selector(stat.Value);
        public bool IsEmpty => stat.IsEmpty;
    }
}