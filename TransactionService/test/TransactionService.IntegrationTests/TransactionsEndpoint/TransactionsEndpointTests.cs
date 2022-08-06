using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using TransactionService.Domain.Models;
using TransactionService.Dtos;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.IntegrationTests.WebApplicationFactories;
using Xunit;

namespace TransactionService.IntegrationTests.TransactionsEndpoint
{
    [Collection("IntegrationTests")]
    public class TransactionsEndpointTests : IClassFixture<MoneyMateApiWebApplicationFactory>, IAsyncLifetime
    {
        private readonly HttpClient HttpClient;
        private readonly DynamoDbHelper DynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#Transaction";

        public TransactionsEndpointTests(MoneyMateApiWebApplicationFactory factory)
        {
            HttpClient = factory.CreateDefaultClient();
            DynamoDbHelper = new DynamoDbHelper();
        }

        public async Task InitializeAsync()
        {
            await DynamoDbHelper.CreateTable();
        }

        public async Task DisposeAsync()
        {
            await DynamoDbHelper.DeleteTable();
        }

        [Fact]
        public async Task GivenNoInputParameters_WhenGetTransactionsIsCalled_ThenAllTransactionsAreReturned()
        {
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = new DateTime(2020, 02, 01).ToString("o"),
                TransactionType = "income",
                Amount = 1223.45M,
                Category = "Test Category",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Salary"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name1",
                Note = "this is a note123"
            };
            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "income",
                Amount = 123.45M,
                Category = "Salary",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Salary"
            };


            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var response = await HttpClient.GetAsync("/api/transactions");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(transactionList, returnedTransactionList);
        }

        [Fact]
        public async Task GivenStartDateInputParameter_WhenGetTransactionsIsCalled_ThenCorrectTransactionsAreReturned()
        {
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = new DateTime(2021, 3, 28).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = new DateTime(2021, 3, 28).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };

            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = new DateTime(2021, 3, 1).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                Note = "this is a note123"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3
            };

            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["start"] = new DateTime(2021, 3, 27).ToString("O");
            var queryString = query.ToString();

            var response = await HttpClient.GetAsync($"/api/transactions?{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2
            };
            Assert.Equal(expectedTransactionList, returnedTransactionList);
        }

        [Fact]
        public async Task GivenEndDateInputParameter_WhenGetTransactionsIsCalled_ThenCorrectTransactionsAreReturned()
        {
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionTimestamp = new DateTime(2021, 3, 1).ToString("O"),
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = new DateTime(2021, 3, 2).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };
            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = new DateTime(2021, 3, 2).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };

            var transaction4 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915d",
                TransactionTimestamp = new DateTime(2021, 3, 5).ToString("O"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                Note = "this is a note123"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3,
                transaction4
            };

            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["end"] = new DateTime(2021, 3, 3).ToString("O");
            var queryString = query.ToString();

            var response = await HttpClient.GetAsync($"/api/transactions?{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            returnedTransactionList!.Sort((transaction, transaction5) =>
                String.CompareOrdinal(transaction.TransactionId, transaction5.TransactionId));

            var expectedTransactionList = new List<Transaction>
            {
                transaction1, transaction2, transaction3
            };
            Assert.Equal(expectedTransactionList, returnedTransactionList);
        }

        [Fact]
        public async Task GivenTypeInputParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfTypeAreReturned()
        {
            var transactionType = "expense";
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = transactionType,
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name1",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "income",
                Amount = 123.45M,
                Category = "Income",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Salary"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["type"] = transactionType;
            var queryString = query.ToString();

            var response = await HttpClient.GetAsync($"/api/transactions?{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<Transaction> { transaction1 }, returnedTransactionList);
        }

        [Fact]
        public async Task GivenPayerPayeeIdInputParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfTypeAreReturned()
        {
            var transactionType = "expense";
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = transactionType,
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name1",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "income",
                Amount = 123.45M,
                Category = "Income",
                Subcategory = "Salary"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["payerPayeeId"] = "";
            var queryString = query.ToString();

            var response = await HttpClient.GetAsync($"/api/transactions?{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<Transaction> { transaction2 }, returnedTransactionList);
        }

        [Fact]
        public async Task
            GivenCategoriesParameter_WhenGetTransactionsIsCalled_ThenAllTransactionsOfCategoryAreReturned()
        {
            var transactionType = "expense";
            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915a",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = transactionType,
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name1",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Salary"
            };
            var transaction3 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915c",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "income",
                Amount = 123.45M,
                Category = "Income",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Salary"
            };
            var transaction4 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915d",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Eating Out",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name2",
                Subcategory = "Dinner"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2,
                transaction3,
                transaction4
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            var queryString = "categories=Groceries&categories=Eating Out";

            var response = await HttpClient.GetAsync($"/api/transactions?{queryString}");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedTransactionList = JsonSerializer.Deserialize<List<Transaction>>(returnedString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            Assert.Equal(new List<Transaction> { transaction1, transaction2, transaction4 }, returnedTransactionList);
        }

        [Fact]
        public async Task GivenInvalidRequest_WhenPostTransactionsIsCalled_ThenValidationErrorShouldBeReturned()
        {
            var inputDto = new StoreTransactionDto
            {
                Category = "Food",
                Subcategory = "Dinner",
                TransactionTimestamp = "2017-06-21T14:57:17",
                TransactionType = "expense",
                PayerPayeeId = Guid.NewGuid().ToString(),
                PayerPayeeName = "name1",
                Note = "note123"
            };

            string inputDtoString = JsonSerializer.Serialize(inputDto);

            StringContent httpContent =
                new StringContent(inputDtoString, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"/api/transactions", httpContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task
            GivenValidTransactionRequest_WhenPostTransactionsIsCalled_ThenCorrectTransactionsArePersisted()
        {
            const decimal expectedAmount = 123M;
            const string expectedCategory = "Food";
            const string expectedSubcategory = "Dinner";
            var expectedTransactionTimestamp =
                new DateTimeOffset(new DateTime(2021, 4, 2)).ToString("yyyy-MM-ddThh:mm:ss.FFFK");
            const string expectedTransactionType = "expense";
            var expectedPayerPayeeId = Guid.NewGuid().ToString();
            const string expectedPayerPayeeName = "name1";
            const string expectedNote = "this is a note123";
            var inputDto = new StoreTransactionDto
            {
                Amount = expectedAmount,
                Category = expectedCategory,
                Subcategory = expectedSubcategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType,
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                Note = expectedNote
            };

            string inputDtoString = JsonSerializer.Serialize(inputDto);

            StringContent httpContent =
                new StringContent(inputDtoString, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"/api/transactions", httpContent);
            response.EnsureSuccessStatusCode();

            var returnedTransactions = await DynamoDbHelper.ScanTable<Transaction>();

            Assert.Single(returnedTransactions);
            Assert.Equal(expectedAmount, returnedTransactions[0].Amount);
            Assert.Equal(expectedCategory, returnedTransactions[0].Category);
            Assert.Equal(expectedSubcategory, returnedTransactions[0].Subcategory);
            Assert.Equal(expectedTransactionTimestamp, returnedTransactions[0].TransactionTimestamp);
            Assert.Equal(expectedTransactionType, returnedTransactions[0].TransactionType);
            Assert.Equal(expectedPayerPayeeId, returnedTransactions[0].PayerPayeeId);
            Assert.Equal(expectedPayerPayeeName, returnedTransactions[0].PayerPayeeName);
            Assert.Equal(expectedNote, returnedTransactions[0].Note);
        }

        [Fact]
        public async Task GivenValidPutTransactionDto_WhenPutTransactionsIsCalled_ThenCorrectTransactionsArePersisted()
        {
            var expectedTransactionId = Guid.NewGuid().ToString();

            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = expectedTransactionId,
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat",
                Note = "this is a note123"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);

            const decimal expectedAmount = 123M;
            const string expectedCategory = "Food";
            const string expectedSubcategory = "Dinner";
            var expectedTransactionTimestamp =
                new DateTimeOffset(new DateTime(2021, 4, 2)).ToString("yyyy-MM-ddThh:mm:ss.FFFK");
            const string expectedTransactionType = "expense";
            const string expectedPayerPayeeId = "id123";
            const string expectedPayerPayeeName = "name123";
            const string expectedNote = "This is a new note";
            var inputDto = new PutTransactionDto
            {
                Amount = expectedAmount,
                Category = expectedCategory,
                Subcategory = expectedSubcategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType,
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                Note = expectedNote
            };

            string inputDtoString = JsonSerializer.Serialize(inputDto);
            StringContent httpContent = new StringContent(inputDtoString, Encoding.UTF8, "application/json");

            var response = await HttpClient.PutAsync($"/api/transactions/{expectedTransactionId}", httpContent);
            response.EnsureSuccessStatusCode();

            var returnedTransaction = await DynamoDbHelper.QueryTable<Transaction>(UserId, expectedTransactionId);
            var expectedTransaction = new Transaction
            {
                UserId = UserId,
                TransactionId = expectedTransactionId,
                Amount = expectedAmount,
                Category = expectedCategory,
                Subcategory = expectedSubcategory,
                TransactionTimestamp = expectedTransactionTimestamp,
                TransactionType = expectedTransactionType,
                PayerPayeeId = expectedPayerPayeeId,
                PayerPayeeName = expectedPayerPayeeName,
                Note = expectedNote
            };
            Assert.Equal(expectedTransaction, returnedTransaction);
        }

        [Fact]
        public async Task GivenValidTransactionId_WhenDeleteTransactionIsCalled_ThenCorrectTransactionIsDeleted()
        {
            var expectedTransactionId = Guid.NewGuid().ToString();

            var transaction1 = new Transaction
            {
                UserId = UserId,
                TransactionId = expectedTransactionId,
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };
            var transaction2 = new Transaction
            {
                UserId = UserId,
                TransactionId = "fa00567c-468e-4ccf-af4c-fca1c731915b",
                TransactionTimestamp = DateTime.Now.ToString("o"),
                TransactionType = "expense",
                Amount = 123.45M,
                Category = "Groceries",
                Subcategory = "Meat"
            };

            var transactionList = new List<Transaction>
            {
                transaction1,
                transaction2
            };
            await DynamoDbHelper.WriteTransactionsIntoTable(transactionList);
            var response = await HttpClient.DeleteAsync($"/api/transactions/{expectedTransactionId}");
            response.EnsureSuccessStatusCode();

            var returnedTransaction = await DynamoDbHelper.QueryTable<Transaction>(UserId, expectedTransactionId);
            Assert.Null(returnedTransaction);
        }
    }
}