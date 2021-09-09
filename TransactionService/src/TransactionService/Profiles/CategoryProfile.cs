using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Dtos;

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