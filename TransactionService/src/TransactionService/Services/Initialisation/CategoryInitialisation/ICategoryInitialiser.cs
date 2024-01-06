using System;
using System.Threading.Tasks;

namespace TransactionService.Services.Initialisation.CategoryInitialisation;

public interface ICategoryInitialiser
{
    public Task InitialiseCategories(Guid userId, Guid profileId);
}