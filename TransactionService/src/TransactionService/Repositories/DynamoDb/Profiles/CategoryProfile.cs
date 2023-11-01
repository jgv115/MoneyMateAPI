using TransactionService.Domain.Models;
using TransactionService.Repositories.DynamoDb.Models;
using Profile = AutoMapper.Profile;

namespace TransactionService.Repositories.DynamoDb.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<DynamoDbCategory, Category>();
            CreateMap<Category, DynamoDbCategory>();
        }
    }
}