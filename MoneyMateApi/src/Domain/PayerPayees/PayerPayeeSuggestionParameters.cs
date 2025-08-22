using System;
using MoneyMateApi.Controllers.PayersPayees.Dtos;

namespace MoneyMateApi.Domain.Services.PayerPayees;

public record CategoryPayerPayeeSuggestionParameters(string Category)
    : IPayerPayeeSuggestionParameters;

public record SubcategoryPayerPayeeSuggestionParameters(string Category, string Subcategory)
    : IPayerPayeeSuggestionParameters;

public record GeneralPayerPayeeSuggestionParameters : IPayerPayeeSuggestionParameters;

public class PayerPayeeSuggestionParameterFactory
{
    public IPayerPayeeSuggestionParameters Generate(
        SuggestionPromptDto suggestionPromptDto)
    {
        IPayerPayeeSuggestionParameters suggestionParameters = new GeneralPayerPayeeSuggestionParameters();
        if (suggestionPromptDto.PromptType == SuggestionPromptType.All)
        {
            if (!string.IsNullOrEmpty(suggestionPromptDto.Category) ||
                !string.IsNullOrEmpty(suggestionPromptDto.Subcategory))
                throw new ArgumentException("Suggestion Prompt values cannot be provided if prompt type is 'All'");
        }

        if (suggestionPromptDto.PromptType == SuggestionPromptType.Category)
        {
            if (string.IsNullOrEmpty(suggestionPromptDto.Category))
                throw new ArgumentException(
                    "Suggestion Prompt value for Category cannot be empty if prompt type is 'Category'");

            suggestionParameters = new CategoryPayerPayeeSuggestionParameters(suggestionPromptDto.Category);
        }

        if (suggestionPromptDto.PromptType == SuggestionPromptType.Subcategory)
        {
            if (string.IsNullOrEmpty(suggestionPromptDto.Category) ||
                string.IsNullOrEmpty(suggestionPromptDto.Subcategory))
                throw new ArgumentException(
                    "Suggestion Prompt values for Category and Subcateogry must be provided  if prompt type is 'Subcategory'");
            suggestionParameters = new SubcategoryPayerPayeeSuggestionParameters(suggestionPromptDto.Category,
                suggestionPromptDto.Subcategory);
        }

        return suggestionParameters;
    }
}