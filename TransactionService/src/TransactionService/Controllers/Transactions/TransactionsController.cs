using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Services.Transactions;

namespace TransactionService.Controllers.Transactions
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionHelperService _transactionHelperService;

        public TransactionsController(ITransactionHelperService transactionHelperService)
        {
            _transactionHelperService = transactionHelperService;
        }

        // GET api/transactions
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetTransactionsQuery queryParams
        )
        {
            var result = await _transactionHelperService
                .GetTransactionsAsync(queryParams);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(StoreTransactionDto storeTransactionDto)
        {
            await _transactionHelperService.StoreTransaction(storeTransactionDto);
            return Ok();
        }

        [HttpPut("{transactionId}")]
        public async Task<IActionResult> Put(string transactionId, PutTransactionDto putTransactionDto)
        {
            await _transactionHelperService.PutTransaction(transactionId, putTransactionDto);
            return Ok();
        }

        [HttpDelete("{transactionId}")]
        public async Task<IActionResult> Delete(string transactionId)
        {
            await _transactionHelperService.DeleteTransaction(transactionId);
            return NoContent();
        }
    }
}