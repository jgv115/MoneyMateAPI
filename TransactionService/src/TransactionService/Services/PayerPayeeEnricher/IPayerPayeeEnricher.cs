using System.Threading.Tasks;
using TransactionService.Services.PayerPayeeEnricher.Models;

namespace TransactionService.Services.PayerPayeeEnricher
{
    public interface IPayerPayeeEnricher
    {
        public Task<ExtraPayerPayeeDetails> GetExtraPayerPayeeDetails(string identifier);
    }
}