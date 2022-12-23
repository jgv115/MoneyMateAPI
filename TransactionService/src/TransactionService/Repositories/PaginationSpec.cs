namespace TransactionService.Repositories
{
    public record PaginationSpec
    {
        public int Offset { get; init; }
        public int Limit { get; init; }
    }
}