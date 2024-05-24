using System.Threading.Tasks;
using TransactionService.Constants;
using TransactionService.Controllers.PayersPayees.ViewModels;
using TransactionService.Domain.Models;
using TransactionService.Services.PayerPayeeEnricher.Models;

namespace TransactionService.Services.PayerPayeeEnricher
{
    public interface IPayerPayeeEnricher
    {
        public Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier);

        public Task<PayerPayeeViewModel> EnrichPayerPayeeToViewModel(PayerPayeeType payerPayeeType,
            PayerPayee payerPayeeToBeEnriched);
    }
}