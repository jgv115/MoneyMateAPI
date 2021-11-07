using System;
using TransactionService.Helpers.TimePeriodHelpers;
using Xunit;

namespace TransactionService.Tests.Helpers.TimePeriodHelpers
{
    public class TimePeriodTests
    {
        public class Constructor
        {
            [Fact]
            public void GivenValidInputs_WhenConstructorInvoked_ThenPeriodAndFrequencySetCorrectly()
            {
                const string timeFrequency = "WEEK";
                const int timePeriods = 54;

                var timePeriod = new TimePeriod(timeFrequency, timePeriods);

                Assert.Equal(new TimePeriod
                {
                    TimeFrequency = TimeFrequency.WEEK,
                    NumFrequencyPeriods = timePeriods
                }, timePeriod);
            }

            [Fact]
            public void GivenInvalidTimeFrequency_WhenConstructorInvoked_ThenArgumentExceptionThrown()
            {
                Assert.Throws<ArgumentException>(() => new TimePeriod("invalid", 432));
            }
        }
    }
}