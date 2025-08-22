using System.Threading.Tasks;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Domain.Services.Tags;

public interface ITagService
{
    public Task<Tag> CreateTag(string name);
}