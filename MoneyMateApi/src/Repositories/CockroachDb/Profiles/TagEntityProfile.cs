namespace MoneyMateApi.Repositories.CockroachDb.Profiles;

public class TagEntityProfile: AutoMapper.Profile
{
    public TagEntityProfile()
    {
        CreateMap<Entities.Tag, Domain.Models.Tag>();
    }   
}