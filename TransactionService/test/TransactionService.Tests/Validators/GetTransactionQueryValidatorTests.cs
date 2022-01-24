using System;
using System.Collections.Generic;
using FluentValidation.TestHelper;
using TransactionService.Constants;
using TransactionService.Dtos;
using TransactionService.Validators;
using Xunit;

namespace TransactionService.Tests.Validators;

public class GetTransactionQueryValidatorTests
{
    private readonly GetTransactionQueryValidator _validator = new();

    [Fact]
    public void GivenCategoriesAndSubcategoryQueryAreNotEmpty_ThenValidationShouldFail()
    {
        var input = new GetTransactionsQuery
        {
            Type = TransactionType.Expense,
            Start = DateTime.MinValue,
            End = DateTime.Today,
            Categories = new List<string> {"hello", "hello"},
            SubcategoriesQuery = new List<string> {"hello", "hello"}
        };

        var result = _validator.TestValidate(input);
        result.ShouldHaveValidationErrorFor(query => query.SubcategoriesQuery);
    }

    [Fact]
    public void GivenCategoriesAndSubcategoryQueryAreBothEmpty_ThenValidationShouldNotFail()
    {
        var input = new GetTransactionsQuery
        {
            Type = TransactionType.Expense,
            Start = DateTime.MinValue,
            End = DateTime.Today,
        };

        var result = _validator.TestValidate(input);
        result.ShouldNotHaveAnyValidationErrors();
    }
}