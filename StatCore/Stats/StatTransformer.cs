using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatCore.Stats
{
    public class StatTransformer<TIn, TMid, TOut> : IStat<TIn, TOut>
    {
        private readonly IStat<TIn, TMid> stat;
        private readonly Func<TMid, TOut> selector;

        public StatTransformer(IStat<TIn, TMid> stat, Func<TMid, TOut> selector)
        {
            this.stat = stat;
            this.selector = selector;
        }

        public void Add(TIn item)
        {
            stat.Add(item);
        }

        public void Delete(TIn item)
        {
            stat.Delete(item);
        }

        public TOut Value => selector(stat.Value);
        public bool IsEmpty => stat.IsEmpty;
    }
}
