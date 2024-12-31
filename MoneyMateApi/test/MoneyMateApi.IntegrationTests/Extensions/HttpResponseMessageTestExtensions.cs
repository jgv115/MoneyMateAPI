using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MoneyMateApi.IntegrationTests.Extensions;

public static class HttpResponseMessageTestExtensions
{
    public static async Task AssertSuccessfulStatusCode(this HttpResponseMessage httpResponseMessage)
    {
        if (!httpResponseMessage.IsSuccessStatusCode)
        {
            var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
            Assert.Fail(
                $"Received a non successful status code: {(int) httpResponseMessage.StatusCode} with body: {responseBody}");
        }
    }
}