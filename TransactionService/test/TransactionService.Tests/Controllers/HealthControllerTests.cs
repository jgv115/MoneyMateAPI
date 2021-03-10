using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Controllers;
using Xunit;

namespace TransactionService.Tests.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public void GetHealthShouldReturn200()
        {
            var controller = new HealthController();
            var response = controller.Get();
            
            var objectResult = Assert.IsType<OkResult>(response);
            
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
        }
    }
}