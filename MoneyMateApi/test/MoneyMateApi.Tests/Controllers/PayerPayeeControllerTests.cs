using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.Exceptions;
using MoneyMateApi.Controllers.PayersPayees;
using MoneyMateApi.Controllers.PayersPayees.Dtos;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Services.PayerPayees;
using Xunit;

namespace MoneyMateApi.Tests.Controllers
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
            Assert.Throws<ArgumentNullException>(() => new PayersPayeesController(null!));
        }

        [Theory]
        [InlineData(-1, 10, "Invalid offset value")]
        [InlineData(0, 21, "Invalid limit value")]
        [InlineData(0, -1, "Invalid limit value")]
        public async Task GivenInvalidOffset_WhenGetPayersInvoked_ThenQueryParameterInvalidExceptionThrown(int offset,
            int limit, string exceptionMessage)
        {
            var repository = new PayersPayeesController(_mockService.Object);
            var exception =
                await Assert.ThrowsAsync<QueryParameterInvalidException>(() => repository.GetPayers(offset, limit));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task GivenDefaultArguments_WhenGetPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payers = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            const int offset = 5;
            const int limit = 20;
            const bool includeEnrichedData = true;
            _mockService.Setup(service => service.GetPayers(offset, limit, includeEnrichedData)).ReturnsAsync(() => payers);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayers(offset, limit);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }
        
        [Fact]
        public async Task GivenIncludeEnrichedDataIsFalse_WhenGetPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payers = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            const int offset = 5;
            const int limit = 20;
            _mockService.Setup(service => service.GetPayers(offset, limit, false)).ReturnsAsync(() => payers);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayers(offset, limit, false);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }

        [Theory]
        [InlineData(-1, 10, "Invalid offset value")]
        [InlineData(0, 21, "Invalid limit value")]
        [InlineData(0, -1, "Invalid limit value")]
        public async Task GivenInvalidOffset_WhenGetPayeesInvoked_ThenQueryParameterInvalidExceptionThrown(int offset,
            int limit, string exceptionMessage)
        {
            var repository = new PayersPayeesController(_mockService.Object);
            var exception =
                await Assert.ThrowsAsync<QueryParameterInvalidException>(() => repository.GetPayees(offset, limit));

            Assert.Equal(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task GivenOffsetAndLimitNotProvided_WhenGetPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payers = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            _mockService.Setup(service => service.GetPayers(0, 10, true)).ReturnsAsync(() => payers);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayers();
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }


        [Fact]
        public async Task GivenDefaultArguments_WhenGetPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            const int offset = 5;
            const int limit = 20;
            _mockService.Setup(service => service.GetPayees(offset, limit, true)).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayees(offset, limit);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }
        
        [Fact]
        public async Task GivenIncludeEnrichedDataIsFalse_WhenGetPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            const int offset = 5;
            const int limit = 20;
            _mockService.Setup(service => service.GetPayees(offset, limit, false)).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayees(offset, limit, false);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        [Fact]
        public async Task GivenOffsetAndLimitNotProvided_WhenGetPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            _mockService.Setup(service => service.GetPayees(0, 10, true)).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetPayees();
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        #region GetSuggestedPayers

        [Fact]
        public async Task
            GivenRequestWithAllSuggestionPromptType_WhenGetSuggestedPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payers = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                        new SuggestionPromptDto(SuggestionPromptType.All, null, null), true))
                .ReturnsAsync(payers);
            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayers(new SuggestionPromptDto(SuggestionPromptType.All, null, null), true);

            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }

        #endregion

        #region GetSuggestedPayees

        [Fact]
        public async Task
            GivenRequest_WhenGetSuggestedPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            var includeEnrichedData = true;
            
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                        new SuggestionPromptDto(SuggestionPromptType.All, null, null), includeEnrichedData))
                .ReturnsAsync(payees);
            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayees(new SuggestionPromptDto(), includeEnrichedData);

            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }
        
        [Fact]
        public async Task
            GivenRequestWithoutIsEnrichedDataProvided_WhenGetSuggestedPayeesInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };
            
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payee,
                        new SuggestionPromptDto(SuggestionPromptType.All, null, null), true))
                .ReturnsAsync(payees);
            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayees(new SuggestionPromptDto());

            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenArgumentExceptionReturnedFromService_WhenGetSuggestedPayeesInvoked_ThenReturns400BadRequestReturned()
        {
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payee, new SuggestionPromptDto(
                        SuggestionPromptType.Subcategory, "test", null), true))
                .ThrowsAsync(new ArgumentException("invalid arguments"));

            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayees(new SuggestionPromptDto(SuggestionPromptType.Subcategory, "test"));

            var objectResponse = Assert.IsType<BadRequestObjectResult>(response);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResponse.StatusCode);
        }

        #endregion

        #region GetSuggestedPayers

        [Fact]
        public async Task
            GivenRequest_WhenGetSuggestedPayersInvoked_ThenReturns200OKWithCorrectList()
        {
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test name1"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test name1"
                }
            };

            var includeEnrichedData = false;
            
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payer,
                        new SuggestionPromptDto(SuggestionPromptType.All, null, null), includeEnrichedData))
                .ReturnsAsync(payees);
            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayers(new SuggestionPromptDto(), includeEnrichedData);

            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenArgumentExceptionReturnedFromService_WhenGetSuggestedPayersInvoked_ThenReturns400BadRequestReturned()
        {
            _mockService.Setup(service =>
                    service.GetSuggestedPayersOrPayees(PayerPayeeType.Payer, new SuggestionPromptDto(
                        SuggestionPromptType.Subcategory, "test", null), true))
                .ThrowsAsync(new ArgumentException("invalid arguments"));

            var controller = new PayersPayeesController(_mockService.Object);

            var response =
                await controller.GetSuggestedPayers(new SuggestionPromptDto(SuggestionPromptType.Subcategory, "test"), true);

            var objectResponse = Assert.IsType<BadRequestObjectResult>(response);

            Assert.Equal(StatusCodes.Status400BadRequest, objectResponse.StatusCode);
        }

        #endregion
        
        [Fact]
        public async Task GivenWhitespaceInputName_WhenGetAutocompletePayerInvoked_ThenReturns400BadRequest()
        {
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetAutocompletePayer("");
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task
            GivenPayerPayeeServiceReturnsPayers_WhenGetAutocompletePayerInvoked_ThenReturns200OKWithCorrectList()
        {
            var inputName = "test";
            var payers = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test123"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test1234"
                }
            };

            _mockService.Setup(service => service.AutocompletePayer(inputName)).ReturnsAsync(() => payers);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetAutocompletePayer(inputName);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payers, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenPayerPayeeServiceReturnsPayees_WhenGetAutocompletePayeeInvoked_ThenReturns200OKWithCorrectList()
        {
            var inputName = "test";
            var payees = new List<PayerPayeeViewModel>
            {
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id123",
                    PayerPayeeName = "test123"
                },
                new()
                {
                    PayerPayeeId = Guid.NewGuid(),
                    ExternalId = "id1234",
                    PayerPayeeName = "test1234"
                }
            };

            _mockService.Setup(service => service.AutocompletePayee(inputName)).ReturnsAsync(() => payees);
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetAutocompletePayee(inputName);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(payees, objectResponse.Value);
        }

        [Fact]
        public async Task GivenWhitespaceInputName_WhenGetAutocompletePayeeInvoked_ThenReturns400BadRequest()
        {
            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.GetAutocompletePayee("");
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task
            GivenValidPayerPayeeIdAndPayerPayeeServiceReturnsObject_WhenGetPayerInvoked_ThenReturns200OKWithCorrectPayer()
        {
            var expectedPayer = new PayerPayeeViewModel
            {
                ExternalId = "externalId",
                PayerPayeeId = Guid.NewGuid(),
                PayerPayeeName = "name"
            };
            _mockService.Setup(service => service.GetPayer(It.IsAny<Guid>())).ReturnsAsync(() => expectedPayer);
            var controller = new PayersPayeesController(_mockService.Object);

            var response = await controller.GetPayer(Guid.NewGuid());
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedPayer, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidPayerPayeeIdAndPayerPayeeServiceReturnsObject_WhenGetPayeeInvoked_ThenReturns200OKWithCorrectPayee()
        {
            var expectedPayee = new PayerPayeeViewModel
            {
                ExternalId = "externalId",
                PayerPayeeId = Guid.NewGuid(),
                PayerPayeeName = "name"
            };
            _mockService.Setup(service => service.GetPayee(It.IsAny<Guid>())).ReturnsAsync(() => expectedPayee);
            var controller = new PayersPayeesController(_mockService.Object);

            var response = await controller.GetPayee(Guid.NewGuid());
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedPayee, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidCreatePayerPayeeDto_WhenPostPayerInvoked_ThenPayerPayeeServiceCreatePayerCalledWithCorrectDto()
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
        public async Task GivenNoErrors_WhenPostPayerInvoked_Then200OKReturnedWithCorrectObject()
        {
            var name = "test name";
            var externalId = "test external id";


            var dto = new CreatePayerPayeeDto
            {
                Name = name,
                ExternalId = externalId
            };

            var expectedViewModel = new PayerPayeeViewModel
            {
                ExternalId = externalId,
                PayerPayeeName = name,
                PayerPayeeId = Guid.NewGuid()
            };
            _mockService.Setup(service => service.CreatePayer(dto)).ReturnsAsync(() => expectedViewModel);

            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.PostPayer(dto);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedViewModel, objectResponse.Value);
        }

        [Fact]
        public async Task
            GivenValidCreatePayerPayeeDto_WhenPostPayeeInvoked_ThenPayerPayeeServiceCreatePayeeCalledWithCorrectDto()
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
        public async Task GivenNoErrors_WhenPostPayeeInvoked_Then200OKReturnedWithCorrectObject()
        {
            var name = "test name";
            var externalId = "test external id";

            var dto = new CreatePayerPayeeDto
            {
                Name = name,
                ExternalId = externalId
            };

            var expectedViewModel = new PayerPayeeViewModel
            {
                ExternalId = externalId,
                PayerPayeeName = name,
                PayerPayeeId = Guid.NewGuid()
            };
            _mockService.Setup(service => service.CreatePayee(dto)).ReturnsAsync(() => expectedViewModel);

            var controller = new PayersPayeesController(_mockService.Object);
            var response = await controller.PostPayee(dto);
            var objectResponse = Assert.IsType<OkObjectResult>(response);

            Assert.Equal(StatusCodes.Status200OK, objectResponse.StatusCode);
            Assert.Equal(expectedViewModel, objectResponse.Value);
        }
    }
}