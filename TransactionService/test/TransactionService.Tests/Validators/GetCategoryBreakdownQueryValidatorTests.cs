using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using TransactionService.Dtos;
using TransactionService.Validators;
using Xunit;

namespace TransactionService.Tests.Validators
{
    public class GetCategoryBreakdownQueryValidatorTests
    {
        private readonly GetCategoryBreakdownQueryValidator _validator = new();

        [Fact]
        public void GivenEmptyType_ThenShouldThrowValidationException()
        {
            var request = new GetCategoryBreakdownQuery();
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(query => query.Type);
        }

        [Fact]
        public void GivenStartDateAndEndDateHasValue_ThenFrequencyMustBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Start = DateTime.MinValue,
                End = DateTime.MaxValue,
                Frequency = "MONTHS",
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Frequency);
        }

        [Fact]
        public void GivenStartDateAndEndDateHasValue_ThenPeriodsMustBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Start = DateTime.MinValue,
                End = DateTime.MaxValue,
                Periods = 1
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Periods);
        }

        [Fact]
        public void GivenStartDateHasValue_ThenEndDateShouldNotBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Start = DateTime.MinValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.End);
        }

        [Fact]
        public void GivenEndDateHasValue_ThenStartDateShouldNotBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                End = DateTime.MaxValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Start);
        }

        [Fact]
        public void GivenFrequencyAndPeriodsHasValue_ThenStartMustBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Start = DateTime.MinValue,
                Periods = 1,
                Frequency = "MONTHS",
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Start);
        }

        [Fact]
        public void GivenFrequencyAndPeriodsHasValue_ThenEndMustBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                End = DateTime.MaxValue,
                Periods = 1,
                Frequency = "MONTHS",
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.End);
        }

        [Fact]
        public void GivenFrequencyHasValue_ThenPeriodsShouldNotBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Frequency = "MONTHS",
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Periods);
        }

        [Fact]
        public void GivenPeriodsHasValue_ThenFrequencyShouldNotBeEmpty()
        {
            var input = new GetCategoryBreakdownQuery
            {
                Type = "expense",
                Periods = 1,
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Frequency);
        }
    }
}