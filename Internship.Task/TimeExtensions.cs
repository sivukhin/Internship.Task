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

        public static string ToUtcFormat(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }
    }
}
