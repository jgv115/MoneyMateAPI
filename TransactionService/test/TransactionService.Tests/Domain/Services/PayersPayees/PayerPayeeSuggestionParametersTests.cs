using System;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Domain.Services.PayerPayees;
using Xunit;

namespace TransactionService.Tests.Domain.Services.PayersPayees;

public class PayerPayeeSuggestionParameterFactoryTests
{
    #region "All" Suggestion Prompt Type

    [Fact]
    public void GivenAllSuggestionPromptType_WhenGenerateInvoked_ThenGeneralSuggestionParametersReturned()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        var suggestionParameters = factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.All,
        });

        Assert.Equal(new GeneralPayerPayeeSuggestionParameters(), suggestionParameters);
    }

    [Fact]
    public void
        GivenAllSuggestionPromptTypeWithInvalidInputs_WhenGenerateInvoked_ThenArgumentExceptionThrown()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        Assert.Throws<ArgumentException>(() => factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.All,
            Category = "test",
            Subcategory = "test2"
        }));
    }

    [Fact]
    public void GivenCategorySuggestionPromptType_WhenGenerateInvoked_ThenCategorySuggestionParametersReturned()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        var suggestionParameters = factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.Category,
            Category = "test category"
        });

        Assert.Equal(new CategoryPayerPayeeSuggestionParameters("test category"), suggestionParameters);
    }

    [Fact]
    public void
        GivenCategorySuggestionPromptTypeWithNoCategoryInput_WhenGenerateInvoked_ThenArgumentExceptionThrown()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        Assert.Throws<ArgumentException>(() => factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.Category,
        }));
    }


    [Fact]
    public void GivenSubcategorySuggestionPromptType_WhenGenerateInvoked_ThenSubcategorySuggestionParametersReturned()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        var suggestionParameters = factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.Subcategory,
            Category = "test category",
            Subcategory = "subcategory1"
        });

        Assert.Equal(new SubcategoryPayerPayeeSuggestionParameters("test category", "subcategory1"),
            suggestionParameters);
    }

    [Fact]
    public void
        GivenSubcategorySuggestionPromptTypeWithNoSubctegoryInput_WhenGenerateInvoked_ThenArgumentExceptionThrown()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        Assert.Throws<ArgumentException>(() => factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.Subcategory,
            Category = "category"
        }));
    }

    [Fact]
    public void
        GivenSubcategorySuggestionPromptTypeWithNoCategoryInput_WhenGenerateInvoked_ThenArgumentExceptionThrown()
    {
        var factory = new PayerPayeeSuggestionParameterFactory();

        Assert.Throws<ArgumentException>(() => factory.Generate(new SuggestionPromptDto
        {
            PromptType = SuggestionPromptType.Subcategory,
            Subcategory = "subcategory"
        }));
    }

    #endregion
}