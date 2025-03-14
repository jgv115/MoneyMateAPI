using System;
using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.Repositories.CockroachDb.Profiles
{
    public class TransactionEntityProfile : AutoMapper.Profile
    {
        public TransactionEntityProfile()
        {
            CreateMap<Transaction, Domain.Models.Transaction>()
                .ForMember(transaction => transaction.TransactionId,
                    opt => opt.MapFrom(transaction => transaction.Id))
                .ForMember(transaction => transaction.TransactionTimestamp,
                    opt => opt.MapFrom(transaction => transaction.TransactionTimestamp.ToString("o")))
                .ForMember(transaction => transaction.PayerPayeeId,
                    opt => opt.MapFrom(transaction =>
                        transaction.PayerPayeeId == Guid.Empty ? null : transaction.PayerPayeeId.ToString()));
        }
    }
}