using MoneyMateApi.Controllers.Categories.Dtos;
using MoneyMateApi.Domain.Categories;
using Profile = AutoMapper.Profile;

namespace MoneyMateApi.Profiles
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