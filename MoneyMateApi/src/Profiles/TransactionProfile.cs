using System.Linq;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Transactions;
using Profile = AutoMapper.Profile;

namespace MoneyMateApi.Profiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<StoreTransactionDto, Transaction>();
            CreateMap<PutTransactionDto, Transaction>();
        }
    }
}