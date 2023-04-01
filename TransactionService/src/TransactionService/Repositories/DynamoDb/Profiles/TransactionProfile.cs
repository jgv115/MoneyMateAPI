using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Repositories.DynamoDb.Models;

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