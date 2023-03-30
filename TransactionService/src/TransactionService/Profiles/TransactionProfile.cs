using AutoMapper;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Repositories.DynamoDb.Models;

namespace TransactionService.Profiles
{
    public class TransactionProfile: Profile
    {
        public TransactionProfile()
        {
            CreateMap<StoreTransactionDto, Transaction>();
            CreateMap<PutTransactionDto, Transaction>();
        }
    }
}