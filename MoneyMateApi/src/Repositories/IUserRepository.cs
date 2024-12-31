using System.Threading.Tasks;
using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.Repositories;

public interface IUserRepository
{
    public Task<User> GetUser();
}