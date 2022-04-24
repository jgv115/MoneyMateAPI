using TransactionService.Constants;
using TransactionService.Domain.Models;

namespace TransactionService.Tests.Common;

public class TransactionListBuilder
{
    private readonly string _userId;
    private readonly List<Transaction> _transactionList;

    public TransactionListBuilder(string userId)
    {
        _userId = userId;
        _transactionList = new List<Transaction>();
    }

    public List<Transaction> Build()
    {
        return _transactionList;
    }

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndAmount(int number, string category,
        decimal amount)
    {
        for (var i = 0; i < number; i++)
        {
            _transactionList.Add(new Transaction
            {
                UserId = _userId,
                Amount = amount,
                Category = category,
                TransactionTimestamp = DateTime.Now.ToString("O"),
                Subcategory = "subcategory-1",
                TransactionId = Guid.NewGuid().ToString(),
                TransactionType = "expense",
            });
        }

        return this;
    }

    public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number,
        string category, string subcategory,
        decimal amount)
    {
        for (var i = 0; i < number; i++)
        {
            _transactionList.Add(new Transaction
            {
                UserId = _userId,
                Amount = amount,
                Category = category,
                TransactionTimestamp = DateTime.Now.ToString("O"),
                Subcategory = subcategory,
                TransactionId = Guid.NewGuid().ToString(),
                TransactionType = "expense",
            });
        }

        return this;
    }

    public TransactionListBuilder WithNumberOfTransactionsOfPayerPayeeIdAndPayerPayeeName(int number, Guid? payerPayeeId,
        string? payerPayeeName, decimal amount, TransactionType transactionType = TransactionType.Expense)
    {
        for (var i = 0; i < number; i++)
        {
            _transactionList.Add(new Transaction
            {
                UserId = _userId,
                Amount = amount,
                Category = "category123",
                TransactionTimestamp = DateTime.Now.ToString("O"),
                Subcategory = "subcategory123",
                TransactionId = Guid.NewGuid().ToString(),
                TransactionType = "expense",
                PayerPayeeId = payerPayeeId?.ToString(),
                PayerPayeeName = payerPayeeName
            });
        }

        return this;
    }
}