using System;

namespace MoneyMateApi.IntegrationTests.Helpers;

public class TransactionTypeIds
{
    public Guid Expense { get; init; }
    public Guid Income { get; init; }

    public TransactionTypeIds(Guid expenseId, Guid incomeId)
    {
        Expense = expenseId;
        Income = incomeId;
    }
}