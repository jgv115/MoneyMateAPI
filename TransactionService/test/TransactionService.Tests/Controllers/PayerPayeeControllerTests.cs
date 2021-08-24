using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TransactionService.Controllers;
using TransactionService.Domain;
using TransactionService.Dtos;
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
                    ExternalId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    ExternalId = "id1234"
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
                    ExternalId = "id123"
                },
                new()
                {
                    Name = "test1234",
                    UserId = "userId1234",
                    ExternalId = "id1234"
                }
            };

            _mockService.Setup(service => service.GetPayees()).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayees();
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        [Fact]
        public async Task GivenValidCreatePayerPayeeDto_WhenPostPayerInvoked_ThenPayerPayeeServiceCreatePayerCalledWithCorrectDto()
        {
            var dto = new CreatePayerPayeeDto
            {
                Name = "test name",
                ExternalId = "test external id"
            };
            var controller = new PayersPayeesController(_mockService.Object);
            await controller.PostPayer(dto);
            
            _mockService.Verify(service => service.CreatePayer(dto));
        }

        [Fact]
        public async Task GivenNoErrors_WhenPostPayerInvoked_Then200OKReturned()
        {
            var dto = new CreatePayerPayeeDto
            {
                Name = "test name",
                ExternalId = "test external id"
            };
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.PostPayer(dto);
            var objectResponse = Assert.IsType<OkResult>(response);
            
            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }
        
        [Fact]
        public async Task GivenValidCreatePayerPayeeDto_WhenPostPayeeInvoked_ThenPayerPayeeServiceCreatePayeeCalledWithCorrectDto()
        {
            var dto = new CreatePayerPayeeDto
            {
                Name = "test name",
                ExternalId = "test external id"
            };
            var controller = new PayersPayeesController(_mockService.Object);
            await controller.PostPayee(dto);
            
            _mockService.Verify(service => service.CreatePayee(dto));
        }
        
        [Fact]
        public async Task GivenNoErrors_WhenPostPayeeInvoked_Then200OKReturned()
        {
            var dto = new CreatePayerPayeeDto
            {
                Name = "test name",
                ExternalId = "test external id"
            };
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.PostPayee(dto);
            var objectResponse = Assert.IsType<OkResult>(response);
            
            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
        }
    }
}