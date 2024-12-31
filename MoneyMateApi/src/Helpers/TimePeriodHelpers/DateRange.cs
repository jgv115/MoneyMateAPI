using System;

namespace MoneyMateApi.Helpers.TimePeriodHelpers
{
    public record DateRange
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public DateRange(DateTime start, DateTime end)
        {
            if (start >= end)
            {
                throw new ArgumentException("Start date must be before the end date");
            }

            Start = start;
            End = end;
        }
    }
}