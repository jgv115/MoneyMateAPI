using TransactionService.Constants;
using TransactionService.Domain.Models;

namespace TransactionService.Tests.Common;

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
        WithTransactions(number, null, null, amount, null, category, null);

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number, string category,
        string subcategory, decimal amount) =>
        WithTransactions(number, null, null, amount, null, category, subcategory);

    public TransactionListBuilder WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(int number,
        string? payerPayeeId,
        string? payerPayeeName, decimal amount, TransactionType transactionType = TransactionType.Expense) =>
        WithTransactions(number, payerPayeeId, payerPayeeName, amount, transactionType, null, null);

    public TransactionListBuilder WithTransactions(int number, string? payerPayeeId, string? payerPayeeName,
        decimal? amount, TransactionType? transactionType, string? category, string? subcategory)
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
                PayerPayeeName = payerPayeeName ?? ""
            });
        }

        return this;
    }
}