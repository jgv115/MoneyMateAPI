using System;
using System.Collections.Generic;

namespace MoneyMateApi.Repositories.CockroachDb.Entities
{
    public record UserProfiles
    {
        public string UserIdentifier { get; set; }
        public List<Guid> ProfileIds { get; init; }
    }
}