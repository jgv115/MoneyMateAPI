namespace TransactionService.Domain.Services.PayerPayees;

public record SubcategoryPayerPayeeSuggestionParameters(string Category, string Subcategory)
    : IPayerPayeeSuggestionParameters;

public record GeneralPayerPayeeSuggestionParameters : IPayerPayeeSuggestionParameters;