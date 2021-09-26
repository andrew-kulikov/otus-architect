using System;
using FeedHistory.BarsGenerator.Models;

namespace FeedHistory.BarsGenerator
{
    public static class Extensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetNextDate(this long time, BarPeriod barPeriod) =>
            GetAppender(barPeriod)
                .Invoke(GetDate(time))
                .ToTimestamp();

        private static DateTime GetDate(long timestamp) => Epoch.AddMilliseconds(timestamp);

        public static long ToTimestamp(this DateTime date) => (long)(date - Epoch).TotalMilliseconds;

        private static Func<DateTime, DateTime> GetAppender(BarPeriod barPeriod) =>
            barPeriod switch
            {
                BarPeriod.M1 => date => date.AddMinutes(1),
                BarPeriod.M5 => date => date.AddMinutes(5),
                BarPeriod.M15 => date => date.AddMinutes(15),
                BarPeriod.M30 => date => date.AddMinutes(30),
                BarPeriod.H1 => date => date.AddMinutes(60),
                BarPeriod.H4 => date => date.AddMinutes(240),
                BarPeriod.D1 => date => date.AddDays(1),
                BarPeriod.W1 => date => date.AddDays(7),
                BarPeriod.Mo1 => date => date.AddMonths(1),
                _ => throw new ArgumentOutOfRangeException(nameof(barPeriod), barPeriod, null)
            };

        public static int GetEstimatedMinutes(this BarPeriod period) =>
            period switch
            {
                BarPeriod.M1 => 1,
                BarPeriod.M5 => 5,
                BarPeriod.M15 => 15,
                BarPeriod.M30 => 30,
                BarPeriod.H1 => 60,
                BarPeriod.H4 => 240,
                BarPeriod.D1 => 1440,
                BarPeriod.W1 => 10080,
                BarPeriod.Mo1 => 43200,
                _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
            };
    }
}