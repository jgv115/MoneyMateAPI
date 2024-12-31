using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyMateApi.Controllers.Transactions.Dtos;
using MoneyMateApi.Domain.Services.Transactions;

namespace MoneyMateApi.Controllers.Transactions
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

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetById(string transactionId)
        {
            var transaction = await _transactionHelperService.GetTransactionById(transactionId);
            return Ok(transaction);
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