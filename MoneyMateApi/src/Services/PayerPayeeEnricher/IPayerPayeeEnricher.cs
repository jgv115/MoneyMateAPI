using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;
using MoneyMateApi.Services.PayerPayeeEnricher.Models;

namespace MoneyMateApi.Services.PayerPayeeEnricher
{
    public interface IPayerPayeeEnricher
    {
        public Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier);

        public Task<PayerPayeeViewModel> EnrichPayerPayeeToViewModel(PayerPayeeType payerPayeeType,
            PayerPayee payerPayeeToBeEnriched);
    }
}