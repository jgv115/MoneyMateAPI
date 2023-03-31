using AutoMapper;
using TransactionService.Domain.Models;
using TransactionService.Repositories.DynamoDb.Models;

namespace TransactionService.Repositories.DynamoDb.Profiles
{
    public class PayerPayeeProfile : Profile
    {
        public PayerPayeeProfile()
        {
            CreateMap<DynamoDbPayerPayee, PayerPayee>().ConvertUsing<DynamoDbPayerPayeeToPayerPayeeConverter>();
            CreateMap<PayerPayee, DynamoDbPayerPayee>();
        }
    }

    public class DynamoDbPayerPayeeToPayerPayeeConverter : ITypeConverter<DynamoDbPayerPayee, PayerPayee>
    {
        private string ExtractPayerPayeeId(string rangeKey) => rangeKey.Split("#")[1];

        public PayerPayee Convert(DynamoDbPayerPayee source, PayerPayee destination, ResolutionContext context)
        {
            return new PayerPayee
            {
                ExternalId = source.ExternalId,
                PayerPayeeId = ExtractPayerPayeeId(source.PayerPayeeId),
                PayerPayeeName = source.PayerPayeeName
            };
        }
    }
}