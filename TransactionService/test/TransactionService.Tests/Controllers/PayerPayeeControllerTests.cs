using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Domain;
using TransactionService.Models;
using Xunit;

namespace TransactionService.Tests.Controllers
{
    public class PayerPayeeControllerTests
    {
        private readonly Mock<IPayerPayeeService> _mockService;

        public PayerPayeeControllerTests()
        {
            _mockService = new Mock<IPayerPayeeService>();
        }

        [Fact]
        public void GivenNullPayerPayeeService_WhenConstructorInvoked_ThenThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PayersPayeesController(null));
        }

        [Fact]
        public async Task GivenPayerPayeeServiceReturnsObject_WhenGetPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payers = new List<PayerPayee>
            {
                new()
                {
                    Name = "test123",
                    UserId = "userId123",
                    GooglePlaceId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    GooglePlaceId = "id1234"
                }
            };

            _mockService.Setup(service => service.GetPayers()).ReturnsAsync(() => payers);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayers();
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }
        
        [Fact]
        public async Task GivenPayerPayeeServiceReturnsObject_WhenGetPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayee>
            {
                new()
                {
                    Name = "test123",
                    UserId = "userId123",
                    GooglePlaceId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    GooglePlaceId = "id1234"
                }
            };

            _mockService.Setup(service => service.GetPayees()).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayees();
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }
    }
}