namespace MoneyMateApi.Helpers.TimePeriodHelpers
{
    public interface ITimePeriodHelper
    {
        public DateRange ResolveDateRange(TimePeriod timePeriod);
    }
}