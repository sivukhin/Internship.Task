using System;
using System.Diagnostics;

namespace Kontur.GameStats.Server
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
