using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TransactionService.IntegrationTests.Extensions
{
    public static class HttpClientExtensions
    {
        public static void GetAccessToken(this HttpClient httpClient)
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

            Console.WriteLine($">>> responseString {responseString}");
            var deserializedObject = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);
            var accessToken = deserializedObject["access_token"].ToString();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}