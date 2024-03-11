using System.Threading.Tasks;
using Xunit;

namespace TransactionService.IntegrationTests.Repositories.CockroachDb;

public class CockroachDbPayerPayeeRepositoryTests: IAsyncLifetime
{
    public CockroachDbPayerPayeeRepositoryTests()
    {
        
    }
    
    public Task InitializeAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task DisposeAsync()
    {
        throw new System.NotImplementedException();
    }
}