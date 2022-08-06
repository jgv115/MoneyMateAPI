using System;
using System.Globalization;
using FluentDateTime;
using Microsoft.Extensions.Internal;

namespace TransactionService.Helpers.TimePeriodHelpers
{
    public static class DateTimeExtensions
    {
        public static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }

    public class TimePeriodHelper : ITimePeriodHelper
    {
        private readonly ISystemClock _systemClock;

        public TimePeriodHelper(ISystemClock systemClock)
        {
            _systemClock = systemClock;
        }

        public DateRange ResolveDateRange(TimePeriod timePeriod)
        {
            var timeFrequency = timePeriod.TimeFrequency;
            var periods = timePeriod.NumFrequencyPeriods;
            var currentDateTime = _systemClock.UtcNow.DateTime;

            DateTime endDate = DateTime.MaxValue;
            DateTime startDate = DateTime.MinValue;

            switch (timeFrequency)
            {
                case TimeFrequency.WEEK:
                    if (periods == 0)
                    {
                        endDate = currentDateTime.EndOfDay();
                        startDate = currentDateTime.GetStartOfWeek(DayOfWeek.Monday);
                    }
                    else
                    {
                        endDate = currentDateTime.AddDays(-7).GetStartOfWeek(DayOfWeek.Monday).AddDays(6).EndOfDay();
                        startDate = currentDateTime.AddDays(-7 * periods).GetStartOfWeek(DayOfWeek.Monday);
                    }
                    break;

                case TimeFrequency.MONTH:
                    if (periods == 0)
                    {
                        endDate = currentDateTime.EndOfDay();
                        startDate = currentDateTime.BeginningOfMonth();
                    }
                    else
                    {
                        endDate = currentDateTime.AddMonths(-1).EndOfMonth();
                        startDate = currentDateTime.AddMonths(-1 * periods).BeginningOfMonth();
                    }
                    break;
            }

            return new DateRange(startDate, endDate);
        }
    }
}