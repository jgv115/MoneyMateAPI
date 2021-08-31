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
        
        [HttpGet("payers/{payerPayeeId:guid}")]
        public async Task<IActionResult> GetPayer(Guid payerPayeeId)
        {
            var payer = await _payerPayeeService.GetPayer(payerPayeeId);
            return Ok(payer);
        }
        
        [HttpGet("payees/{payerPayeeId:guid}")]
        public async Task<IActionResult> GetPayee(Guid payerPayeeId)
        {
            var payee = await _payerPayeeService.GetPayee(payerPayeeId);
            return Ok(payee);
        }

        [HttpPost("payers")]
        public async Task<IActionResult> PostPayer(CreatePayerPayeeDto createPayerPayeeDto)
        {
            await _payerPayeeService.CreatePayer(createPayerPayeeDto);
            return Ok();
        }
        
        [HttpPost("payees")]
        public async Task<IActionResult> PostPayee(CreatePayerPayeeDto createPayerPayeeDto)
        {
            await _payerPayeeService.CreatePayee(createPayerPayeeDto);
            return Ok();
        }
    }
}