using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Repositories.CockroachDb.Entities;

namespace TransactionService.Repositories
{
    public interface IProfilesRepository
    {
        public Task<List<Profile>> GetProfiles();
    }
}