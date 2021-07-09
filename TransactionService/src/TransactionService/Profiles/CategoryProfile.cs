using AutoMapper;
using TransactionService.Dtos;
using TransactionService.Models;

namespace TransactionService.Profiles
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CreateCategoryDto, Category>();
        }
    }
}