using System.Threading.Tasks;
using MoneyMateApi.Constants;
using MoneyMateApi.Controllers.PayersPayees.ViewModels;
using MoneyMateApi.Domain.Models;

namespace MoneyMateApi.Services.PayerPayeeEnricher
{
    public interface IPayerPayeeEnricher
    {
        public Task<PayerPayeeViewModel> EnrichPayerPayeeToViewModel(PayerPayeeType payerPayeeType,
            PayerPayee payerPayeeToBeEnriched);
    }
}