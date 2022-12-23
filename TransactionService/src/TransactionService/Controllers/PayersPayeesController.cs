using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Domain.Services;
using TransactionService.Domain.Services.PayerPayees;
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

        [HttpGet("payers")]
        public async Task<IActionResult> GetPayers()
        {
            var payers = await _payerPayeeService.GetPayers();
            return Ok(payers);
        }

        [HttpGet("payers/autocomplete")]
        public async Task<IActionResult> GetAutocompletePayer(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            else
            {
                var payers = await _payerPayeeService.AutocompletePayer(name);
                return Ok(payers);
            }
        }

        [HttpGet("payees")]
        public async Task<IActionResult> GetPayees()
        {
            var payees = await _payerPayeeService.GetPayees();
            return Ok(payees);
        }

        [HttpGet("payees/autocomplete")]
        public async Task<IActionResult> GetAutocompletePayee(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))
            {
                return BadRequest();
            }
            else
            {
                var payers = await _payerPayeeService.AutocompletePayee(name);
                return Ok(payers);
            }
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
            var createdPayer = await _payerPayeeService.CreatePayer(createPayerPayeeDto);
            return Ok(createdPayer);
        }

        [HttpPost("payees")]
        public async Task<IActionResult> PostPayee(CreatePayerPayeeDto createPayerPayeeDto)
        {
            var createdPayee = await _payerPayeeService.CreatePayee(createPayerPayeeDto);
            return Ok(createdPayee);
        }
    }
}