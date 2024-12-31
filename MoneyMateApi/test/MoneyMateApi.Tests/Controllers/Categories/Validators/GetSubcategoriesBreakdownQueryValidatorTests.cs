using System;
using FluentValidation.TestHelper;
using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Controllers.Categories.Validators;
using Xunit;

namespace MoneyMateApi.Tests.Controllers.Categories.Validators
{
    public class GetSubcategoriesBreakdownQueryValidatorTests
    {
        private readonly GetSubcategoriesBreakdownQueryValidator _validator = new();

        [Fact]
        public void GivenEmptyCategoryName_ThenShouldThrowValidationException()
        {
            var request = new GetSubcategoriesBreakdownQuery();
            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(query => query.Category);
        }

        [Fact]
        public void GivenStartDateHasValue_ThenEndDateShouldNotBeEmpty()
        {
            var input = new GetSubcategoriesBreakdownQuery
            {
                Category = "categoryname",
                Start = DateTime.MinValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.End);
        }

        [Fact]
        public void GivenEndDateHasValue_ThenStartDateShouldNotBeEmpty()
        {
            var input = new GetSubcategoriesBreakdownQuery
            {
                Category = "categoryname",
                End = DateTime.MaxValue
            };
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.Start);
        }
    }
}