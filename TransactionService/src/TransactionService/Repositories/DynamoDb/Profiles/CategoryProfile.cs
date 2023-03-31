using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Repositories.DynamoDb.Models;

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