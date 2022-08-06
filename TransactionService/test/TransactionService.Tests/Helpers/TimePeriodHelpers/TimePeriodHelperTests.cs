using System;
using System.Collections;
using System.Collections.Generic;
using FluentDateTime;
using Microsoft.Extensions.Internal;
using Moq;
using TransactionService.Helpers.TimePeriodHelpers;
using Xunit;

namespace TransactionService.Tests.Helpers.TimePeriodHelpers
{
    public class ResolveDateRange
    {
        private readonly Mock<ISystemClock> _mockSystemClock;

        public ResolveDateRange()
        {
            _mockSystemClock = new Mock<ISystemClock>();
        }

        [Theory]
        [ClassData(typeof(ResolveDateRangeTestData))]
        public void GivenTimePeriodInput_ThenCorrectDateRangeIsReturned(DateTimeOffset currentDateTime, TimePeriod inputTimePeriod, DateRange expectedDateRange)
        {
            _mockSystemClock.Setup(clock => clock.UtcNow).Returns(currentDateTime);
            var helper = new TimePeriodHelper(_mockSystemClock.Object);

            var returnedDateRange = helper.ResolveDateRange(inputTimePeriod);

            Assert.Equal(expectedDateRange, returnedDateRange);
        }
    }

    public class ResolveDateRangeTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] {
                new DateTimeOffset(new DateTime(2010, 5, 28), TimeSpan.Zero),
                new TimePeriod("MONTH", 0),
                new DateRange(new DateTime(2010, 5, 1), new DateTime(2010, 5, 28).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2010, 5, 31)),
                new TimePeriod("MONTH", 0),
                new DateRange(new DateTime(2010, 5, 1), new DateTime(2010, 5, 31).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2000, 2, 3)),
                new TimePeriod("MONTH", 2),
                new DateRange(new DateTime(1999, 12, 1), new DateTime(2000, 1, 31).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2000, 12, 20)),
                new TimePeriod("MONTH", 3),
                new DateRange(new DateTime(2000, 9, 1), new DateTime(2000, 11, 30).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2000, 2, 28)),
                new TimePeriod("MONTH", 5),
                new DateRange(new DateTime(1999, 9, 1), new DateTime(2000, 1, 31).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2010, 5, 1)),
                new TimePeriod("MONTH", 5),
                new DateRange(new DateTime(2009, 12, 1), new DateTime(2010, 4, 30).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2021, 10, 15)),
                new TimePeriod("WEEK", 0),
                new DateRange(new DateTime(2021, 10, 11), new DateTime(2021, 10, 15).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2021, 10, 17)),
                new TimePeriod("WEEK", 0),
                new DateRange(new DateTime(2021, 10, 11), new DateTime(2021, 10, 17).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2021, 10, 14)),
                new TimePeriod("WEEK", 0),
                new DateRange(new DateTime(2021, 10, 11), new DateTime(2021, 10, 14).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2021, 10, 14)),
                new TimePeriod("WEEK", 2),
                new DateRange(new DateTime(2021, 9, 27), new DateTime(2021, 10, 10).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2019, 4, 10)),
                new TimePeriod("WEEK", 1),
                new DateRange(new DateTime(2019, 4, 1), new DateTime(2019, 4, 7).EndOfDay())
            };
            yield return new object[] {
                new DateTimeOffset(new DateTime(2019, 3, 7)),
                new TimePeriod("WEEK", 3),
                new DateRange(new DateTime(2019, 2, 11), new DateTime(2019, 3, 3).EndOfDay())
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}