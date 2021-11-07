using System;
namespace TransactionService.Helpers.TimePeriodHelpers
{
    public enum TimeFrequency
    {
        WEEK, MONTH
    }

    public record TimePeriod
    {
        public TimeFrequency TimeFrequency { get; set; }
        public int NumFrequencyPeriods { get; set; }

        public TimePeriod() { }
        public TimePeriod(string timeFrequency, int numFrequencyPeriods)
        {
            // TODO: might need some validation here
            TimeFrequency = Enum.Parse<TimeFrequency>(timeFrequency);
            NumFrequencyPeriods = numFrequencyPeriods;
        }

    }
}