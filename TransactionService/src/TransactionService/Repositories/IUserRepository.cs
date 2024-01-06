using System.Threading.Tasks;
using TransactionService.Repositories.CockroachDb.Entities;

namespace TransactionService.Repositories;

public interface IUserRepository
{
    public Task<User> GetUser();
}