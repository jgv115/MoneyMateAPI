using MoneyMateApi.Constants;
using Xunit;

namespace MoneyMateApi.Tests.Constants;

public class TransactionTypeTests
{
    [Theory]
    [InlineData(TransactionType.Expense, 0)]
    [InlineData(TransactionType.Income, 1)]
    public void TransactionTypeEnumMappedToCorrectInt(TransactionType transactionType, int enumMapping)
    {
        Assert.Equal(enumMapping, (int) transactionType);
    }
}