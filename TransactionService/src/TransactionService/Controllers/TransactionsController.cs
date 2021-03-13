using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain;

namespace TransactionService.Controllers
{
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
        public async Task<IActionResult> Get()
        {
            var result = await _transactionHelperService.GetAllTransactionsAsync();
            return Ok(result);
        }
    }
}