using System.Linq;
using AutoMapper;
using TransactionService.Repositories.CockroachDb.Entities;

namespace TransactionService.Repositories.CockroachDb.Profiles
{
    public class CategoryEntityProfile : Profile
    {
        public CategoryEntityProfile()
        {
            CreateMap<Category, Domain.Models.Category>()
                .ForMember(category => category.Subcategories, opt =>
                    opt.MapFrom(
                        category => category.Subcategories.Select(subcategory => subcategory.Name)))
                .ForMember(category => category.CategoryName, opt => opt.MapFrom(category => category.Name));
        }
    }
}