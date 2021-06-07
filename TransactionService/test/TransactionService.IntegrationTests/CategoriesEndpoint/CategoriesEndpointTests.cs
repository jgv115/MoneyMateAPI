using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using TransactionService.IntegrationTests.Extensions;
using TransactionService.IntegrationTests.Helpers;
using TransactionService.Models;
using Xunit;

namespace TransactionService.IntegrationTests.CategoriesEndpoint
{
    [Collection("IntegrationTests")]
    public class CategoriesEndpointTests : IClassFixture<WebApplicationFactory<Startup>>, IAsyncLifetime
    {
        private readonly HttpClient _httpClient;
        private readonly DynamoDbHelper _dynamoDbHelper;
        private const string UserId = "auth0|moneymatetest#Categories";

        public CategoriesEndpointTests(WebApplicationFactory<Startup> factory)
        {
            _httpClient = factory.CreateClient();
            _httpClient.GetAccessToken();
            _dynamoDbHelper = new DynamoDbHelper();
        }

        public async Task InitializeAsync()
        {
            await _dynamoDbHelper.CreateTable();
        }

        public async Task DisposeAsync()
        {
            await _dynamoDbHelper.DeleteTable();
        }

        [Fact]
        public async Task GivenValidRequest_WhenGetCategoriesIsCalled_ThenAllCategoriesAreReturned()
        {
            var expectedCategories = new List<string> {"category1", "category2"};

            await _dynamoDbHelper.WriteIntoTable(expectedCategories.Select(category => new Category
            {
                UserId = UserId,
                CategoryName = category,
                SubCategories = new List<string> {"test1", "test2"}
            }));

            var response = await _httpClient.GetAsync("/api/categories");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedCategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

            Assert.Equal(expectedCategories, returnedCategoriesList);
        }
        
        [Fact]
        public async Task GivenValidRequest_WhenGetSubCategoriesIsCalled_ThenAllSubCategoriesAreReturned()
        {
            var expectedCategories = new List<string> {"category1", "category2", "category3"};
            var expectedSubCategories = new List<string> {"test1", "test2", "test4"};
            
            await _dynamoDbHelper.WriteIntoTable(expectedCategories.Select(category => new Category
            {
                UserId = UserId,
                CategoryName = category,
                SubCategories = expectedSubCategories
            }));

            var response = await _httpClient.GetAsync("/api/categories/category1");
            response.EnsureSuccessStatusCode();

            var returnedString = await response.Content.ReadAsStringAsync();
            var returnedCategoriesList = JsonSerializer.Deserialize<List<string>>(returnedString);

            Assert.Equal(expectedSubCategories, returnedCategoriesList);
        }
    }
}