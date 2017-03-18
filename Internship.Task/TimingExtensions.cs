using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticServer
{
    public static class TimingExtensions
    {
        public static Stopwatch Run(this Stopwatch stopwatch)
        {
            stopwatch.Start();
            return stopwatch;
        }
    }
}
