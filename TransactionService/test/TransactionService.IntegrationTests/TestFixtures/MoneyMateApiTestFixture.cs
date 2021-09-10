using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using Xunit;

namespace TransactionService.IntegrationTests.TestFixtures
{
    public class MoneyMateApiTestFixture: IClassFixture<WebApplicationFactory<Startup>>
    {
        public readonly HttpClient HttpClient;
        public readonly DynamoDbHelper DynamoDbHelper;
        public MoneyMateApiTestFixture(WebApplicationFactory<Startup> factory)
        {
            HttpClient = factory.CreateClient();
            HttpClient.GetAccessToken();
            DynamoDbHelper = new DynamoDbHelper();
        }   
    }
}