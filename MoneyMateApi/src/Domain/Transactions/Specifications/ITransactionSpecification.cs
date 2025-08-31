namespace MoneyMateApi.Domain.Transactions.Specifications
{
    public interface ITransactionSpecification
    {
        public bool IsSatisfied(Transaction item);
    }
}