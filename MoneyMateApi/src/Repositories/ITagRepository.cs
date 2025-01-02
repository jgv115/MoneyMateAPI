using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Repositories;

public interface ITagRepository
{
    public Task<List<Tag>> GetTags();
    public Task<Tag> GetTag(Guid id);
    public Task<Tag> CreateTag(string name);
}