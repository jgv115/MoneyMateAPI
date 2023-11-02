using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Models;

namespace TransactionService.Repositories
{
    public interface IProfilesRepository
    {
        public Task<List<Profile>> GetProfiles();
    }
}