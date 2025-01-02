using MoneyMateApi.Constants;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Tests.Common;

public class TransactionListBuilder
{
    private readonly List<Transaction> _transactionList;

    public TransactionListBuilder()
    {
        _transactionList = new List<Transaction>();
    }

    public List<Transaction> Build()
    {
        return _transactionList;
    }

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndAmount(int number, string category,
        decimal amount) =>
        WithTransactions(number, null, null, amount, null, category, null, null);

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number, string category,
        string subcategory, decimal amount) =>
        WithTransactions(number, null, null, amount, null, category, subcategory, null);

    public TransactionListBuilder WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(int number,
        string? payerPayeeId,
        string? payerPayeeName, decimal amount, TransactionType transactionType = TransactionType.Expense) =>
        WithTransactions(number, payerPayeeId, payerPayeeName, amount, transactionType, null, null, null);

    public TransactionListBuilder WithTransactions(
        int number,
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
                TransactionTimestamp = DateTime.UtcNow.ToString("O"),
                Subcategory = subcategory ?? "subcategory123",
                TransactionId = Guid.NewGuid().ToString(),
                TransactionType = transactionType == null
                    ? TransactionType.Expense.ToProperString()
                    : transactionType.Value.ToProperString(),
                PayerPayeeId = payerPayeeId ?? "",
                PayerPayeeName = payerPayeeName ?? "",
                Note = note ?? "",
                TagIds = tagIds ?? []
            });
        }

        return this;
    }
}