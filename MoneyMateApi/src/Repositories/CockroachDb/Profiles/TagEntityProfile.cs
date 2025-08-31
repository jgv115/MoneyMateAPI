using MoneyMateApi.Domain.Tags;

namespace MoneyMateApi.Repositories.CockroachDb.Profiles;

public class TagEntityProfile: AutoMapper.Profile
{
    public TagEntityProfile()
    {
        CreateMap<Entities.Tag, Tag>();
    }   
}