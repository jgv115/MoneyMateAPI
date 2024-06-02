namespace TransactionService.Controllers.PayersPayees.Dtos
{
    public record CreatePayerPayeeDto
    {
        public string Name { get; init; }
        public string? ExternalId { get; init; }
    }
}