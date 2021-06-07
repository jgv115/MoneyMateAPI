using System;
using Moq;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Repositories
{
    public class DynamoDbCategoriesRepositoryTests
    {
        [Fact]
        public void GivenNullIAmazonDynamoDB_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamoDbCategoriesRepository(null));
        }
    }
}