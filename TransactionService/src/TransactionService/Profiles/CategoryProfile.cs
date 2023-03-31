using AutoMapper;
using TransactionService.Controllers.Categories.Dtos;
using TransactionService.Domain.Models;

namespace TransactionService.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CategoryDto, Category>();
            CreateMap<Category, CategoryDto>();
        }
    }
}