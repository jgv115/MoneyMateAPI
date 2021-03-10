using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using TransactionService;


namespace TransactionService.Tests
{
    public class ValuesControllerTests
    {
        
        [Fact(Skip = "skipping")]
        public async Task TestGet()
        {
            var lambdaFunction = new LambdaEntryPoint();

            var request = new APIGatewayHttpApiV2ProxyRequest
            {
                Version = "2.0",
                RawPath = "api/values",
                RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
                {
                    AccountId = "123456789012",
                    ApiId = "api-id",
                    Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                    {
                        Method = "GET",
                        Path = "api/values",
                        Protocol = "HTTP/1.1"
                    }
                }
            };
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);
            Assert.Equal(200, response.StatusCode);
            Assert.Equal("[\"value1\",\"value2\"]", response.Body);
            
            // Assert.True(response.MultiValueHeaders.ContainsKey("Content-Type"));
            // Assert.Equal("application/json; charset=utf-8", response.MultiValueHeaders["Content-Type"][0]);
        }
    }
}