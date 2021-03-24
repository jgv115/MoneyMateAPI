using System;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Repositories
{
    public class DynamoDbTransactionRepositoryTests
    {
        [Fact]
        public void GivenNullIAmazonDynamoDB_WhenConstructorInvoked_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamoDbTransactionRepository(null));
        }
    }
}