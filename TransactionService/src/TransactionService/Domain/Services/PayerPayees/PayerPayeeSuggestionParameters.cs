namespace TransactionService.Domain.Services.PayerPayees;

public record CategoryPayerPayeeSuggestionParameters(string Category)
    : IPayerPayeeSuggestionParameters;

public record SubcategoryPayerPayeeSuggestionParameters(string Category, string Subcategory)
    : IPayerPayeeSuggestionParameters;

public record GeneralPayerPayeeSuggestionParameters : IPayerPayeeSuggestionParameters;