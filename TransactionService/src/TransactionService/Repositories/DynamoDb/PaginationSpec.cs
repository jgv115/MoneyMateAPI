namespace TransactionService.Repositories.DynamoDb
{
    public record PaginationSpec
    {
        public int Offset { get; init; }
        public int Limit { get; init; }
    }
}