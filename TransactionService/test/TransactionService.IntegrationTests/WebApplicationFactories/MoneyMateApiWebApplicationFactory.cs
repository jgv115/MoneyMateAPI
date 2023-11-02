using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Serilog;
using TransactionService.Constants;
using TransactionService.IntegrationTests.Helpers;

namespace TransactionService.IntegrationTests.WebApplicationFactories
{
    public class MoneyMateApiWebApplicationFactory : WebApplicationFactory<Startup>
    {
        private string _accessToken;
        public readonly CockroachDbIntegrationTestHelper CockroachDbIntegrationTestHelper;
        public Guid TestUserId = Guid.NewGuid();

        public MoneyMateApiWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");
            CockroachDbIntegrationTestHelper = new CockroachDbIntegrationTestHelper(TestUserId);
        }

        private string RequestAccessToken()
        {
            using var accessTokenClient = new HttpClient();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://moneymate-dev.au.auth0.com/oauth/token")
            {
                Content = new FormUrlEncodedContent(
                    new List<KeyValuePair<string, string>>()
                    {
                        new("grant_type", "password"),
                        new("username", "test@moneymate.com"),
                        new("password", "/MONEYMATEtest123"),
                        new("audience", "https://api.dev.moneymate.benong.id.au"),
                        new("scope", "read:sample"),
                        new("client_id", "lXVvnG8YKzsLWJThMqMFUajJ5iSOowMM"),
                        new("client_secret", "Bu97DbAbbMVhvAk3WsPGQEbMSHf7pWsfoI2eAJQzheZ3-Aa4HnFXmHM8RbVqL3R6")
                    })
            };
            var responseString = accessTokenClient.SendAsync(request).Result.Content.ReadAsStringAsync().Result;

            var deserializedObject = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);

            _accessToken = deserializedObject["access_token"].ToString();
            return _accessToken;
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = base.CreateHostBuilder();

            builder.UseSerilog((context, configuration) => { configuration.MinimumLevel.Fatal().WriteTo.Console(); });

            return builder;
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken ?? RequestAccessToken()}");
            // client.DefaultRequestHeaders.Add(Headers.ProfileId, TestUserId.ToString());
        }
    }
}