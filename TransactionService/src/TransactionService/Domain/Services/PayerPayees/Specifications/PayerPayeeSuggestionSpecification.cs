namespace TransactionService.Domain.Services.PayerPayees.Specifications;

public enum PayerPayeeSuggestionCriteria
{
    General,
    Subcategory
}

public record PayerPayeeSuggestionSpecification(PayerPayeeSuggestionCriteria Criteria, string Value);