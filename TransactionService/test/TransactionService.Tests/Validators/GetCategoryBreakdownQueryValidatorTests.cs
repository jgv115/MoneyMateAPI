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

        [Theory]
        [MemberData(nameof(GivenStartDateHasValueTestData))]
        public void GivenStartDateHasValue_ThenOnlyEndDateShouldBePopulated(GetCategoryBreakdownQuery input)
        {
            var result = _validator.TestValidate(input);
            result.ShouldHaveValidationErrorFor(query => query.End);
        }

        public static IEnumerable<object[]> GivenStartDateHasValueTestData =>
            new List<object[]>
            {
                new object[]
                {
                    new GetCategoryBreakdownQuery
                    {
                        Type = "expense",
                        Start = DateTime.MinValue
                    },
                    (GetCategoryBreakdownQuery query) => query.End
                }
            };
    }
}