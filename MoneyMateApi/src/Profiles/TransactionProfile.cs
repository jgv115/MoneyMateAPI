using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Models;
using Profile = AutoMapper.Profile;

namespace MoneyMateApi.Profiles
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