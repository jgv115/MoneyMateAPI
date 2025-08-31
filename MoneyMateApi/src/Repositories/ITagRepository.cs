using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Tags;

namespace MoneyMateApi.Repositories;

public interface ITagRepository
{
    public Task<IEnumerable<Tag>> GetTags();
    public Task<IDictionary<Guid, Tag>> GetTags(IEnumerable<Guid> tagIds);
    public Task<Tag> GetTag(Guid id);
    public Task<Tag> CreateTag(string name);
}