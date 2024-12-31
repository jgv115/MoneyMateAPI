using System;
using System.Threading.Tasks;

namespace MoneyMateApi.Services.Initialisation.CategoryInitialisation;

public interface ICategoryInitialiser
{
    public Task InitialiseCategories(Guid userId, Guid profileId);
}