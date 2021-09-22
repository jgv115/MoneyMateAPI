using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain.Services;
using TransactionService.Dtos;

namespace TransactionService.Controllers
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
        public async Task<IActionResult> Get(
            [FromQuery] DateTime? start = null,
            [FromQuery] DateTime? end = null,
            [FromQuery] string type = null
        )
        {
            var result = await _transactionHelperService
                .GetAllTransactionsAsync(
                    start.GetValueOrDefault(DateTime.MinValue),
                    end.GetValueOrDefault(DateTime.MaxValue),
                    type);
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