using System.Threading.Tasks;
using Moq;
using TransactionService.Constants;
using TransactionService.Middleware;
using TransactionService.Repositories;
using TransactionService.Services;
using Xunit;

namespace TransactionService.Tests.Services;

public class InitialisationServiceTests
{
    private readonly Mock<IProfilesRepository> _mockProfilesRepository = new();

    [Fact]
    public async Task ProfileCreatedWithCorrectProfileName()
    {
        var service = new InitialisationService(new CurrentUserContext(), _mockProfilesRepository.Object);

        await service.Initialise();

        _mockProfilesRepository.Verify(repository => repository.CreateProfile(Defaults.DefaultProfileName));
    }
}