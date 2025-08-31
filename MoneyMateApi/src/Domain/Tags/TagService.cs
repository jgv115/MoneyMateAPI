using System.Threading.Tasks;
using MoneyMateApi.Repositories;

namespace MoneyMateApi.Domain.Tags;

public class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagService(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public Task<Tag> CreateTag(string name)
    {
        return _tagRepository.CreateTag(name);
    }
}