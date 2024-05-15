using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Constants;
using TransactionService.Controllers.Exceptions;
using TransactionService.Controllers.PayersPayees.Dtos;
using TransactionService.Domain.Services.PayerPayees;

namespace TransactionService.Controllers.PayersPayees
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

        private void ValidatePaginationParameters(int offset, int limit)
        {
            if (offset < 0)
                throw new QueryParameterInvalidException("Invalid offset value");
            if (limit < 0 || limit > 20)
                throw new QueryParameterInvalidException("Invalid limit value");
        }

        [HttpGet("payers")]
        public async Task<IActionResult> GetPayers(int offset = 0, int limit = 10)
        {
            ValidatePaginationParameters(offset, limit);

            var payers = await _payerPayeeService.GetPayers(offset, limit);
            return Ok(payers);
        }

        [HttpGet("payers/autocomplete")]
        public async Task<IActionResult> GetAutocompletePayer(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))
                return BadRequest();

            var payers = await _payerPayeeService.AutocompletePayer(name);
            return Ok(payers);
        }

        [HttpGet("payers/suggestions")]
        public async Task<IActionResult> GetSuggestedPayers([FromQuery] SuggestionPromptDto suggestionPromptDto)
        {
            var validationProblemDetails = ValidateSuggestionPromptDto(suggestionPromptDto);

            if (validationProblemDetails != null)
                return BadRequest(validationProblemDetails);
            
            var payers = await _payerPayeeService.GetSuggestedPayersOrPayees(PayerPayeeType.Payer, suggestionPromptDto);
            return Ok(payers);
        }

        [HttpGet("payees")]
        public async Task<IActionResult> GetPayees(int offset = 0, int limit = 10)
        {
            ValidatePaginationParameters(offset, limit);
            var payees = await _payerPayeeService.GetPayees(offset, limit);
            return Ok(payees);
        }

        [HttpGet("payees/autocomplete")]
        public async Task<IActionResult> GetAutocompletePayee(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))

                return BadRequest();

            var payers = await _payerPayeeService.AutocompletePayee(name);
            return Ok(payers);
        }


        [HttpGet("payees/suggestions")]
        public async Task<IActionResult> GetSuggestedPayees([FromQuery] SuggestionPromptDto suggestionPromptDto)
        {
            var validationProblemDetails = ValidateSuggestionPromptDto(suggestionPromptDto);

            if (validationProblemDetails != null)
                return BadRequest(validationProblemDetails);

            var payees = await _payerPayeeService.GetSuggestedPayersOrPayees(PayerPayeeType.Payee, suggestionPromptDto);
            return Ok(payees);
        }

        private ProblemDetails? ValidateSuggestionPromptDto(SuggestionPromptDto suggestionPromptDto)
        {
            var problemDiscovered = false;
            var problemDescription = "";
            if (suggestionPromptDto.PromptType == SuggestionPromptType.All)
            {
                if (!string.IsNullOrEmpty(suggestionPromptDto.Category) ||
                    !string.IsNullOrEmpty(suggestionPromptDto.Subcategory))
                {
                    problemDescription = "Suggestion Prompt values cannot be provided if prompt type is 'All'";
                    problemDiscovered = true;
                }
            }

            if (suggestionPromptDto.PromptType == SuggestionPromptType.Category)
            {
                if (string.IsNullOrEmpty(suggestionPromptDto.Category))
                {
                    problemDescription =
                        "Suggestion Prompt value for Category cannot be empty if prompt type is 'Category'";
                    problemDiscovered = true;
                }

                if (!string.IsNullOrEmpty(suggestionPromptDto.Subcategory))
                {
                    problemDescription =
                        "Suggestion Prompt subcategory value cannot be provided if prompt type is 'Category'";
                    problemDiscovered = true;
                }
            }

            if (suggestionPromptDto.PromptType == SuggestionPromptType.Subcategory)
            {
                if (string.IsNullOrEmpty(suggestionPromptDto.Category) ||
                    string.IsNullOrEmpty(suggestionPromptDto.Subcategory))
                {
                    problemDescription =
                        "Suggestion Prompt values for Category and Subcateogry must be provided  if prompt type is 'Subcategory'";
                    problemDiscovered = true;
                }
            }

            if (problemDiscovered)
                return new ProblemDetails
                {
                    Detail = problemDescription,
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Error"
                };

            return null;
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