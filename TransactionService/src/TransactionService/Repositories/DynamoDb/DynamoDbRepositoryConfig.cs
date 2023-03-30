namespace TransactionService.Repositories.DynamoDb
{
    public record DynamoDbRepositoryConfig
    {
        public string TableName { get; init; }
    }
}