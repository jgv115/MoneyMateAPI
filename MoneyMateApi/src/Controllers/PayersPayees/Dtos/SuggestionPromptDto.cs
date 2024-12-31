namespace MoneyMateApi.Controllers.PayersPayees.Dtos;

public enum SuggestionPromptType
{
    All,
    Category,
    Subcategory
}

public record SuggestionPromptDto(
    SuggestionPromptType PromptType = SuggestionPromptType.All,
    string? Category = null,
    string? Subcategory = null);