using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Models;

namespace TransactionService.Repositories
{
    public class MockTransactionRepository: ITransactionRepository
    {
        public Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return Task<List<Transaction>>.Factory.StartNew(() => new List<Transaction>()
            {
                new()
                {
                    Amount = new decimal(20.2),
                    Category = "category1",
                    Date = "date",
                    SubCategory = "subcategory",
                    TransactionId = "transactionid"
                },
                new ()
                {
                    Amount = new decimal(22.2),
                    Category = "category2",
                    Date = "date2",
                    SubCategory = "subcategory2",
                    TransactionId = "transactionid2"
                }
            });
        }        
    }
}