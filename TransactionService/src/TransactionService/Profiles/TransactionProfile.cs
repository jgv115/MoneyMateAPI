using AutoMapper;
using TransactionService.Dtos;
using TransactionService.Models;

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