using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.Repositories.CockroachDb.Profiles
{
    public class PayerPayeeEntityProfile : AutoMapper.Profile
    {
        public PayerPayeeEntityProfile()
        {
            CreateMap<PayerPayee, Domain.Models.PayerPayee>()
                .ForMember(payerPayee => payerPayee.PayerPayeeId, opt =>
                    opt.MapFrom(
                        payerPayee => payerPayee.Id))
                .ForMember(payerPayee => payerPayee.PayerPayeeName, opt =>
                    opt.MapFrom(payerPayee => payerPayee.Name))
                .ForMember(payerPayee => payerPayee.ExternalId, opt =>
                    opt.MapFrom(payerPayee => payerPayee.ExternalLinkId));
        }
    }
}