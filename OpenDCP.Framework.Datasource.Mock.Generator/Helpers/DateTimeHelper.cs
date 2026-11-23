using System;

namespace OpenDCP.Framework.Datasource.Mock.Generator.Helpers
{
    public static class DateTimeHelper
    {
        public static TimeSpan ParseIntervalToTimeSpan(string interval)
        {
            if (string.IsNullOrWhiteSpace(interval))
                throw new ArgumentException("Interval cannot be null or empty.", nameof(interval));

            interval = interval.Trim().ToLowerInvariant();

            if (interval.EndsWith("ms"))
            {
                if (double.TryParse(interval.Substring(0, interval.Length - 2), out double value))
                    return TimeSpan.FromMilliseconds(value);
            }
            else if (interval.EndsWith("mo"))
            {
                if (double.TryParse(interval.Substring(0, interval.Length - 2), out double value))
                    return TimeSpan.FromDays(value * 30);
            }
            else
            {
                char suffix = interval[interval.Length - 1];
                string numberPart = interval.Substring(0, interval.Length - 1);

                if (!double.TryParse(numberPart, out double value))
                    throw new ArgumentException($"Invalid numeric value in interval: {interval}");

                switch (suffix)
                {
                    case 's': return TimeSpan.FromSeconds(value);
                    case 'm': return TimeSpan.FromMinutes(value);
                    case 'h': return TimeSpan.FromHours(value);
                    case 'd': return TimeSpan.FromDays(value);
                    case 'w': return TimeSpan.FromDays(value * 7);

                }
            }

            throw new ArgumentException($"Invalid interval format: {interval}");
        }

    }
}
