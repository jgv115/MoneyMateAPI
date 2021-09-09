using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Dtos;

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