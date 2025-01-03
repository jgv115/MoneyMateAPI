using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Domain.Services.Transactions;

namespace MoneyMateApi.Tests.Common;

/// <summary>
/// This class is responsible for building Transaction domain models and DTOs for testing purposes
/// </summary>
public class TransactionListBuilder
{
    private readonly List<Transaction> _transactionList;
    private readonly HashSet<Guid> _tagIds = [];

    public TransactionListBuilder()
    {
        _transactionList = new List<Transaction>();
    }

    public List<Transaction> BuildDomainModels()
    {
        return _transactionList;
    }

    /// <summary>
    /// Builds a list of TransactionOutputDto objects from the list of transactions.
    /// Defaults tag names to the tag id.
    /// </summary>
    /// <returns></returns>
    public List<TransactionOutputDto> BuildOutputDtos()
    {
        return _transactionList
            .Select(t => t.ToTransactionOutputDto(
                _tagIds.ToDictionary(
                    id => id,
                    id => new Tag(id, id.ToString()))
            )).ToList();
    }

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndAmount(int number, string category,
        decimal amount) =>
        WithTransactions(number, null, null, null, amount, null, category, null, null);

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number, string category,
        string subcategory, decimal amount) =>
        WithTransactions(number, null, null, null, amount, null, category, subcategory, null);

    public TransactionListBuilder WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(int number,
        string? payerPayeeId,
        string? payerPayeeName, decimal amount, TransactionType transactionType = TransactionType.Expense) =>
        WithTransactions(number, null, payerPayeeId, payerPayeeName, amount, transactionType, null, null, null);

    public TransactionListBuilder WithTransactions(
        int number,
        string? transactionId = null,
        string? payerPayeeId = null,
        string? payerPayeeName = null,
        decimal? amount = null,
        TransactionType? transactionType = null,
        string? category = null,
        string? subcategory = null,
        string? note = null,
        List<Guid>? tagIds = null
    )
    {
        for (var i = 0; i < number; i++)
        {
            _transactionList.Add(new Transaction
            {
                Amount = amount ?? 12,
                Category = category ?? "category123",
                TransactionTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Subcategory = subcategory ?? "subcategory123",
                TransactionId = transactionId ?? Guid.NewGuid().ToString(),
                TransactionType = transactionType == null
                    ? TransactionType.Expense.ToProperString()
                    : transactionType.Value.ToProperString(),
                PayerPayeeId = payerPayeeId ?? "",
                PayerPayeeName = payerPayeeName ?? "",
                Note = note ?? "",
                TagIds = tagIds ?? []
            });

            _tagIds.UnionWith(tagIds ?? []);
        }

        return this;
    }
}