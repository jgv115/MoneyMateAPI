using System.Threading.Tasks;

namespace MoneyMateApi.Domain.Tags;

public interface ITagService
{
    public Task<Tag> CreateTag(string name);
}