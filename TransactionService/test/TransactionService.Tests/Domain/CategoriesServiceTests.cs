using System;
using Moq;
using TransactionService.Domain;
using TransactionService.Models;
using TransactionService.Repositories;
using Xunit;

namespace TransactionService.Tests.Domain
{
    public class CategoriesServiceTests
    {
        private readonly Mock<CurrentUserContext> _mockCurrentUserContext;
        private readonly Mock<ICategoriesRepository> _mockRepository;

        public CategoriesServiceTests()
        {
            _mockCurrentUserContext = new Mock<CurrentUserContext>();
            _mockRepository = new Mock<ICategoriesRepository>();
        }

        [Fact]
        public void GivenNullUserContext_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoriesService(null, _mockRepository.Object));
        }

        [Fact]
        public void GivenNullRepository_WhenConstructorInvoked_ThenArgumentNullExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CategoriesService(_mockCurrentUserContext.Object, null));
        }

        [Fact]
        public void
            GivenValidUserContext_WhenGetAllCategoriesInvoked_ThenRepositoryGetAllCategoriesCalledWithCorrectArguments()
        {
            const string expectedUserId = "userId-123";
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

            var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object);

            service.GetAllCategories();

            _mockRepository.Verify(repository => repository.GetAllCategories(expectedUserId));
        }

        [Fact]
        public void
            GivenValidCategoryAndUserContext_WhenGetSubCategoriesInvoked_ThenRepositoryGetSubCategoriesCalledWithCorrectArguments()
        {
            const string expectedUserId = "testUser123";
            const string expectedCategory = "Category1";
            _mockCurrentUserContext.SetupGet(context => context.UserId).Returns(expectedUserId);

            var service = new CategoriesService(_mockCurrentUserContext.Object, _mockRepository.Object);
            service.GetSubCategories(expectedCategory);

            _mockRepository.Verify(repository => repository.GetAllSubCategories(expectedUserId, expectedCategory));
        }
    }
}