using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using TransactionService.Constants;
using TransactionService.Dtos;
using TransactionService.Validators;
using Xunit;

namespace TransactionService.Tests.Validators
{
    public class GetCategoriesBreakdownQueryValidatorTests
    {
        private readonly GetCategoriesBreakdownQueryValidator _validator = new();

        [Fact]
        public void GivenEmptyType_ThenShouldThrowValidationException()
        {
            var request = new GetCategoriesBreakdownQuery();
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(query => query.Type);
        }

        [Fact]
        public void GivenStartDateAndEndDateHasValue_ThenFrequencyMustBeEmpty()
        {
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
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
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
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
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
                Start = DateTime.MinValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.End);
        }

        [Fact]
        public void GivenEndDateHasValue_ThenStartDateShouldNotBeEmpty()
        {
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
                End = DateTime.MaxValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Start);
        }

        [Fact]
        public void GivenFrequencyAndPeriodsHasValue_ThenStartMustBeEmpty()
        {
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
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
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
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
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
                Frequency = "MONTHS",
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Periods);
        }

        [Fact]
        public void GivenPeriodsHasValue_ThenFrequencyShouldNotBeEmpty()
        {
            var input = new GetCategoriesBreakdownQuery
            {
                Type = TransactionType.Expense,
                Periods = 1,
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Frequency);
        }
    }
}