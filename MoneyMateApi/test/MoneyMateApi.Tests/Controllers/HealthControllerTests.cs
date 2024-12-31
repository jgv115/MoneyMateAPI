using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers;
using Xunit;

namespace MoneyMateApi.Tests.Controllers
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