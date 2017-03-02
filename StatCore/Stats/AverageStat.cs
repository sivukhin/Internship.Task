using System;

namespace StatCore.Stats
{
    public class AverageStat<TTarget> : IStat<TTarget, double>
    {
        private AverageValue value;
        private IStat<TTarget, double> Stat { get; set; }
        private readonly Func<TTarget, double> selector;
        public AverageStat(Func<TTarget, double> selector)
        {
            value = new AverageValue();
            this.selector = selector;
        }

        public void Add(TTarget item)
        {
            value += selector(item);
        }

        public void Delete(TTarget item)
        {
            value -= selector(item);
        }

        public double Value => value.Value;
        public bool IsEmpty => Stat.IsEmpty;
    }
}