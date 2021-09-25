using System;

namespace FeedHistory.Common
{
    public static class UtilityExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public static long ToTimestampMilliseconds(this DateTime date) => (long) (date - Epoch).TotalMilliseconds;
        public static DateTime FromTimestampMilliseconds(this long timestamp) => Epoch.AddMilliseconds(timestamp);

        public static bool IsInInterval(DateTime barStart, BarPeriod barPeriod, DateTime time)
        {
            return barPeriod switch
            {
                BarPeriod.M1 => time < barStart.AddMinutes(1),
                BarPeriod.M5 => time < barStart.AddMinutes(5),
                BarPeriod.M15 => time < barStart.AddMinutes(15),
                BarPeriod.M30 => time < barStart.AddMinutes(30),
                BarPeriod.H1 => time < barStart.AddHours(1),
                BarPeriod.H4 => time < barStart.AddHours(4),
                BarPeriod.D1 => time < barStart.AddDays(1),
                BarPeriod.W1 => time < barStart.AddDays(7),
                BarPeriod.Mo1 => time < barStart.AddMonths(1),
                _ => throw new ArgumentOutOfRangeException(nameof(barPeriod), barPeriod, null)
            };
        }

        public static DateTime FindBarStart(DateTime time, BarPeriod barPeriod)
        {
            return barPeriod switch
            {
                BarPeriod.M1 => time.GetMinuteDate(),
                BarPeriod.M5 => GetShiftedMinuteDate(time.GetMinuteDate(), 5),
                BarPeriod.M15 => GetShiftedMinuteDate(time.GetMinuteDate(), 15),
                BarPeriod.M30 => GetShiftedMinuteDate(time.GetMinuteDate(), 30),
                BarPeriod.H1 => time.GetHourDate(),
                BarPeriod.H4 => GetShiftedHourDate(time.GetHourDate(), 4),
                BarPeriod.D1 => time.GetDayDate(),
                BarPeriod.W1 => time.GetWeekStart(),
                BarPeriod.Mo1 => time.GetMonthDate(),
                _ => throw new ArgumentOutOfRangeException(nameof(barPeriod), barPeriod, null)
            };
        }

        public static long FindBarStartMilliseconds(DateTime time, BarPeriod barPeriod) =>
            FindBarStart(time, barPeriod).ToTimestampMilliseconds();

        private static DateTime GetMinuteDate(this DateTime time) =>
            new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0);

        private static DateTime GetHourDate(this DateTime time) =>
            new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);

        private static DateTime GetDayDate(this DateTime time) =>
            new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);

        private static DateTime GetMonthDate(this DateTime time) =>
            new DateTime(time.Year, time.Month, 1, 0, 0, 0);

        private static DateTime GetShiftedMinuteDate(DateTime time, int minutes)
        {
            var offset = time.Minute % minutes;

            return offset == 0
                ? time
                : new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute - offset, 0);
        }

        private static DateTime GetShiftedHourDate(DateTime time, int hours)
        {
            var offset = time.Hour % hours;

            return offset == 0
                ? time
                : new DateTime(time.Year, time.Month, time.Day, time.Hour - offset, 0, 0);
        }

        private static DateTime GetWeekStart(this DateTime time)
        {
            const int startOfWeek = (int) DayOfWeek.Monday;

            //Calculate the number of days it has been since the start of the week
            var daysSinceStartOfWeek = ((int) time.DayOfWeek + 7 - startOfWeek) % 7;

            return time.AddDays(-daysSinceStartOfWeek);
        }
    }
}