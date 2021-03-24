using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain;
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
            [FromQuery] DateTime? end = null
        )
        {
            Console.WriteLine($">>>> {start} {end}");
            var result = await _transactionHelperService
                .GetAllTransactionsAsync(
                    start.GetValueOrDefault(DateTime.MinValue),
                    end.GetValueOrDefault(DateTime.MaxValue));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post(StoreTransactionDto storeTransactionDto)
        {
            await _transactionHelperService.StoreTransaction(storeTransactionDto);
            return Ok();
        }
    }
}