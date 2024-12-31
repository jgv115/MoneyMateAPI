using System.Linq;
using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.Repositories.CockroachDb.Profiles
{
    public class CategoryEntityProfile : AutoMapper.Profile
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