using System;
using FluentDateTime;
using Microsoft.Extensions.Internal;

namespace TransactionService.Helpers.TimePeriodHelpers
{
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
                        startDate = currentDateTime.BeginningOfWeek();
                    }
                    else
                    {
                        endDate = currentDateTime.AddDays(-7).EndOfWeek();
                        startDate = currentDateTime.AddDays(-7 * periods).BeginningOfWeek();
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