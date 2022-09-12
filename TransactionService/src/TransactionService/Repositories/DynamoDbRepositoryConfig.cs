namespace TransactionService.Repositories
{
    public record DynamoDbRepositoryConfig
    {
        public string TableName { get; init; }
    }
}