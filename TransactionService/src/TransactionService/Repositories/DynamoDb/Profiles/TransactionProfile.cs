using TransactionService.Domain.Models;
using TransactionService.Repositories.DynamoDb.Models;
using Profile = AutoMapper.Profile;

namespace TransactionService.Repositories.DynamoDb.Profiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<DynamoDbTransaction, Transaction>();
            CreateMap<Transaction, DynamoDbTransaction>();
        }
    }
}