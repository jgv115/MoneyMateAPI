using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using Profile = AutoMapper.Profile;

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