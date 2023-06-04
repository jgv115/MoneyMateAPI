using System;
using System.Threading.Tasks;
using TransactionService.Repositories;
using TransactionService.Repositories.DynamoDb;
using Xunit;

namespace TransactionService.Tests.Repositories
{
    public class DynamoDbTransactionRepositoryTests
    {
        [Fact]
        public void GivenNullIAmazonDynamoDB_WhenConstructorInvoked_ThenArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamoDbTransactionRepository(null, null, null, null));
        }
    }
}