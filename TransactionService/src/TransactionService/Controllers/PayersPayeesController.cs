using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain;

namespace TransactionService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PayersPayeesController : ControllerBase
    {
        private readonly IPayerPayeeService _payerPayeeService;

        public PayersPayeesController(IPayerPayeeService payerPayeeService)
        {
            _payerPayeeService = payerPayeeService ?? throw new ArgumentNullException(nameof(payerPayeeService));
        }
        
        // GET api/payerspayees/payers
        [HttpGet("payers")]
        public async Task<IActionResult> GetPayers()
        {
            var payers = await _payerPayeeService.GetPayers();
            return Ok(payers);
        }
        
        // GET api/payerspayees/payers
        [HttpGet("payees")]
        public async Task<IActionResult> GetPayees()
        {
            var payees = await _payerPayeeService.GetPayees();
            return Ok(payees);
        }
    }
}