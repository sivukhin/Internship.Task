using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticServer
{
    public static class TimeExtensions
    {
        public static Stopwatch Run(this Stopwatch stopwatch)
        {
            stopwatch.Start();
            return stopwatch;
        }

        public static DateTime ParseUtc(this string dateTimeString)
        {
            return DateTime.Parse(dateTimeString).ToUniversalTime();
        }
    }
}
