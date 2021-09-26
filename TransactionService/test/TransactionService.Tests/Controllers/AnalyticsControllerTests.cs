using System;
using TransactionService.Controllers;
using Xunit;

namespace TransactionService.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        [Fact]
        public void GivenNullService_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new AnalyticsController(null));
        }
    }
}