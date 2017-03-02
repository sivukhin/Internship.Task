using System;
using StatCore.Stats;

namespace StatCore
{
    public static class StatExtensions
    {
        public static IStat<TTarget, TTransform> Select<TTransform, TTarget, TResult>(
            this IStat<TTarget, TResult> stat,
            Func<TResult, TTransform> selector)
        {
            return new StatTransform<TTarget,TTransform,TResult>(
                stat,
                selector);
        }
    }
}