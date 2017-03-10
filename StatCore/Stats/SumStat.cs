using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace StatCore.Stats
{
    public class SumStat<TIn> : IStat<TIn, int>
    {
        private int count;
        private Func<TIn, int> selector;
        public SumStat(Func<TIn, int> selector)
        {
            this.selector = selector;
            Value = 0;
            count = 0;
        }
        public void Add(TIn item)
        {
            Value += selector(item);
            count++;
        }

        public void Delete(TIn item)
        {
            Value -= selector(item);
            count--;
        }

        public int Value { get; private set; }
        public bool IsEmpty => count == 0;
    }
}
