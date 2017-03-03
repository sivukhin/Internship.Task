using System.Collections.Generic;
using StatCore.Stats;

namespace StatCore.Tests
{
    public abstract class BaseStatTest
    {
        protected void HandleEvents<TTarget, TResult>(IStat<TTarget, TResult> stat, params Event<TTarget>[] events)
        {
            foreach (var e in events)
            {
                if (e.IsAddEvent)
                    stat.Add(e.Value);
                else
                    stat.Delete(e.Value);
            }
        }
    }
}