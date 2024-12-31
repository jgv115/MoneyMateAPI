using System;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using Xunit;

namespace MoneyMateApi.Tests.Helpers.TimePeriodHelpers
{
    public class DateRangeTests
    {
        [Fact]
        public void GivenValidInput_WhenConstructorInvoked_ThenStartAndEndPropsSetCorrectly()
        {
            var startDate = DateTime.MinValue;
            var endDate = DateTime.MaxValue;

            var dateRange = new DateRange(startDate, endDate);

            Assert.Equal(startDate, dateRange.Start);
            Assert.Equal(endDate, dateRange.End);
        }

        [Fact]
        public void GivenStartDateAfterEndDate_WhenConstructorInvoked_ThenArgumentExceptionThrown()
        {
            Assert.Throws<ArgumentException>(() => new DateRange(DateTime.MaxValue, DateTime.MinValue));
        }
    }
}