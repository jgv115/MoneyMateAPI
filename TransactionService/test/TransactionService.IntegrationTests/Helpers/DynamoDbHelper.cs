using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using TransactionService.Domain.Models;

namespace TransactionService.IntegrationTests.Helpers
{
    public class DynamoDbHelper
    {
        private readonly AmazonDynamoDBClient _dynamoDbClient;
        private readonly DynamoDBContext _dynamoDbContext;

        private string TableName { get; } =
            $"MoneyMate_TransactionDB_dev";

        public DynamoDbHelper()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.dev.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            var awsSettings = config.GetSection("AWS");
            
            var awsKey = awsSettings.GetValue<string>("AccessKey") ??
                         throw new ArgumentException("Could not find AWS access key for DynamoDb helper");
            var awsSecret = awsSettings.GetValue<string>("SecretKey") ??
                            throw new ArgumentException("Could not find AWS secret key for DynamoDb helper");
            var awsRegion = awsSettings.GetValue<string>("Region") ??
                            throw new ArgumentException("Could not find AWS region for DynamoDb helper");
            var awsUrl = awsSettings.GetValue<string>("ServiceUrl") ??
                         throw new ArgumentException("Could not find AWS URL for DynamoDb helper");
            
            var clientConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = awsUrl,
                AuthenticationRegion = awsRegion
            };

            _dynamoDbClient = new AmazonDynamoDBClient("test", "test", clientConfig);
            _dynamoDbContext = new DynamoDBContext(_dynamoDbClient,
                new DynamoDBOperationConfig { OverrideTableName = TableName });
        }

        public async Task<List<T>> ScanTable<T>()
        {
            return await _dynamoDbContext.ScanAsync<T>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task<Transaction> QueryTable(string hashKey, string rangeKey)
        {
            return await _dynamoDbContext.LoadAsync<Transaction>(hashKey, rangeKey);
        }

        public async Task CreateTable()
        {
            var createTableRequest = new CreateTableRequest
            {
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new()
                    {
                        AttributeName = "UserIdQuery",
                        AttributeType = "S"
                    },
                    new()
                    {
                        AttributeName = "Subquery",
                        AttributeType = "S"
                    },
                    new()
                    {
                        AttributeName = "TransactionTimestamp",
                        AttributeType = "S"
                    },
                    new()
                    {
                        AttributeName = "PayerPayeeName",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new()
                    {
                        AttributeName = "UserIdQuery",
                        KeyType = "HASH"
                    },
                    new()
                    {
                        AttributeName = "Subquery",
                        KeyType = "RANGE"
                    }
                },
                BillingMode = BillingMode.PAY_PER_REQUEST,
                LocalSecondaryIndexes = new List<LocalSecondaryIndex>
                {
                    new()
                    {
                        Projection = new Projection { ProjectionType = ProjectionType.ALL },
                        IndexName = "TransactionTimestampIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new()
                            {
                                AttributeName = "UserIdQuery",
                                KeyType = KeyType.HASH
                            },
                            new()
                            {
                                AttributeName = "TransactionTimestamp",
                                KeyType = KeyType.RANGE
                            }
                        }
                    }
                },
                GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                {
                    new()
                    {
                        Projection = new Projection()
                        {
                            ProjectionType = ProjectionType.INCLUDE,
                            NonKeyAttributes = new List<string> { "ExternalId" }
                        },
                        IndexName = "PayerPayeeNameIndex",
                        KeySchema = new List<KeySchemaElement>
                        {
                            new()
                            {
                                AttributeName = "UserIdQuery",
                                KeyType = KeyType.HASH
                            },
                            new()
                            {
                                AttributeName = "PayerPayeeName",
                                KeyType = KeyType.RANGE
                            }
                        }
                    }
                },
                TableName = "MoneyMate_TransactionDB_dev"
            };

            ListTablesResponse listTableResponse = await _dynamoDbClient.ListTablesAsync();
            if (listTableResponse.TableNames.Contains(createTableRequest.TableName))
            {
                return;
            }

            createTableRequest.TableName = TableName;
            CreateTableResponse response = await _dynamoDbClient.CreateTableAsync(createTableRequest);
            await WaitTillTableCreated(response);
        }

        private async Task WaitTillTableCreated(CreateTableResponse response)
        {
            TableDescription tableDescription = response.TableDescription;
            var status = tableDescription.TableStatus;

            while (status != "ACTIVE")
            {
                Thread.Sleep(500);
                var res = await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                {
                    TableName = TableName
                });
                status = res.Table.TableStatus;
            }
        }

        public async Task DeleteTable()
        {
            await _dynamoDbClient.DeleteTableAsync(new DeleteTableRequest
            {
                TableName = TableName
            });

            while (true)
            {
                try
                {
                    await _dynamoDbClient.DescribeTableAsync(new DescribeTableRequest
                    {
                        TableName = TableName
                    });
                }
                catch (ResourceNotFoundException)
                {
                    break;
                }

                Thread.Sleep(200);
            }
        }

        public async Task WriteTransactionsIntoTable(IEnumerable<Transaction> items)
        {
            // var table = Table.LoadTable(_dynamoDbClient, TableName);
            foreach (var item in items)
            {
                // var itemDoc = Document.FromJson(JsonSerializer.Serialize(item));
                // await table.PutItemAsync(itemDoc);
                await _dynamoDbContext.SaveAsync(item);
            }
        }

        public async Task WriteIntoTable(IEnumerable<dynamic> items)
        {
            foreach (var item in items)
            {
                await _dynamoDbContext.SaveAsync(item);
            }
        }
    }
}