using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Unicode;
using System.Threading.Tasks;
using Moq;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
using TransactionService.Dtos;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Services;
using TransactionService.ViewModels;
using Xunit;

namespace TransactionService.Tests.Services
{
    public class AnalyticsServiceTests
    {
        public class Constructor
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public Constructor()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public void GivenNullTransactionService_ThenArgumentNullExceptionThrown()
            {
                Assert.Throws<ArgumentNullException>(() => new AnalyticsService(null, _mockTimePeriodHelper.Object));
            }

            [Fact]
            public void GivenNullTimePeriodHelper_ThenArgumentNullExceptionThrown()
            {
                Assert.Throws<ArgumentNullException>(() => new AnalyticsService(_mockTransactionService.Object, null));
            }
        }

        public class GetCategoriesBreakdown_ByDateTime
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetCategoriesBreakdown_ByDateTime()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }


            [Fact]
            public async Task
                GivenInputParameters_ThenTransactionServiceCalledWithCorrectArguments()
            {
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>())).ReturnsAsync(() =>
                        new List<Transaction>
                        {
                            new Transaction
                            {
                                Amount = 123M,
                                Category = "category",
                                TransactionTimestamp = DateTime.Now.ToString("O"),
                                Subcategory = "subcategory-1",
                                TransactionId = "transaction-id-1",
                                TransactionType = "expense",
                            }
                        });

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                await service.GetCategoriesBreakdown(expectedType, null, start, end);

                _mockTransactionService.Verify(transactionService => transactionService.GetTransactionsAsync(
                    It.Is<GetTransactionsQuery>(query =>
                        query.Start == start && query.End == end && query.Type == expectedType &&
                        !query.Subcategories.Any() && !query.Categories.Any())
                ));
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                const string expectedCategory1 = "category1";
                const int numberOfCategory1Transactions = 24;
                const decimal category1TransactionsAmount = (decimal)34.5;

                const string expectedCategory2 = "category2";
                const int numberOfCategory2Transactions = 20;
                const decimal category2TransactionsAmount = (decimal)34.5;

                const string expectedCategory3 = "category3";
                const int numberOfCategory3Transactions = 10;
                const decimal category3TransactionsAmount = (decimal)34.5;

                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                        category1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                        category2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                        category3TransactionsAmount)
                    .Build();

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>()))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoriesBreakdown(expectedType, null, start, end);

                var expectedAnalyticsCategories = new List<AnalyticsCategory>
                {
                    new()
                    {
                        CategoryName = expectedCategory1,
                        TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory2,
                        TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory3,
                        TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
                    }
                };

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }

            [Fact]
            public async Task
                GivenInputCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                const int expectedCount = 2;
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                const string expectedCategory1 = "category1";
                const int numberOfCategory1Transactions = 24;
                const decimal category1TransactionsAmount = (decimal)34.5;

                const string expectedCategory2 = "category2";
                const int numberOfCategory2Transactions = 20;
                const decimal category2TransactionsAmount = (decimal)34.5;

                const string expectedCategory3 = "category3";
                const int numberOfCategory3Transactions = 10;
                const decimal category3TransactionsAmount = (decimal)34.5;

                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                        category1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                        category2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                        category3TransactionsAmount)
                    .Build();

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>()))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoriesBreakdown(expectedType, expectedCount, start, end);

                var expectedAnalyticsCategories = new List<AnalyticsCategory>
                {
                    new()
                    {
                        CategoryName = expectedCategory1,
                        TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory2,
                        TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory3,
                        TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
                    }
                }.Take(expectedCount);

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }
        }

        public class GetCategoriesBreakdown_ByTimePeriod
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetCategoriesBreakdown_ByTimePeriod()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public async Task GivenInputParameters_TransactionServiceCalledWithCorrectArguments()
            {
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;
                var timePeriod = new TimePeriod("MONTH", 5);
                var dateRange = new DateRange(start, end);

                _mockTimePeriodHelper.Setup(helper => helper.ResolveDateRange(timePeriod)).Returns(dateRange);

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>())).ReturnsAsync(() =>
                        new List<Transaction>
                        {
                            new Transaction
                            {
                                Amount = 123M,
                                Category = "category",
                                TransactionTimestamp = DateTime.Now.ToString("O"),
                                Subcategory = "subcategory-1",
                                TransactionId = "transaction-id-1",
                                TransactionType = "expense",
                            }
                        });

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                await service.GetCategoriesBreakdown(expectedType, null, timePeriod);

                _mockTransactionService.Verify(transactionService => transactionService.GetTransactionsAsync(
                    It.Is<GetTransactionsQuery>(query =>
                        query.Start == start && query.End == end && query.Type == expectedType &&
                        !query.Subcategories.Any() && !query.Categories.Any())));
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;
                var timePeriod = new TimePeriod("MONTH", 5);
                var dateRange = new DateRange(start, end);

                const string expectedCategory1 = "category1";
                const int numberOfCategory1Transactions = 24;
                const decimal category1TransactionsAmount = (decimal)34.5;

                const string expectedCategory2 = "category2";
                const int numberOfCategory2Transactions = 20;
                const decimal category2TransactionsAmount = (decimal)34.5;

                const string expectedCategory3 = "category3";
                const int numberOfCategory3Transactions = 10;
                const decimal category3TransactionsAmount = (decimal)34.5;

                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                        category1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                        category2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                        category3TransactionsAmount)
                    .Build();

                _mockTimePeriodHelper.Setup(helper => helper.ResolveDateRange(timePeriod)).Returns(dateRange);

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>()))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoriesBreakdown(expectedType, null, timePeriod);

                var expectedAnalyticsCategories = new List<AnalyticsCategory>
                {
                    new()
                    {
                        CategoryName = expectedCategory1,
                        TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory2,
                        TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory3,
                        TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
                    }
                };

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }

            [Fact]
            public async Task
                GivenInputCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                const int expectedCount = 2;
                var expectedType = TransactionType.Expense;
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;
                var timePeriod = new TimePeriod("MONTH", 5);
                var dateRange = new DateRange(start, end);

                const string expectedCategory1 = "category1";
                const int numberOfCategory1Transactions = 24;
                const decimal category1TransactionsAmount = (decimal)34.5;

                const string expectedCategory2 = "category2";
                const int numberOfCategory2Transactions = 20;
                const decimal category2TransactionsAmount = (decimal)34.5;

                const string expectedCategory3 = "category3";
                const int numberOfCategory3Transactions = 10;
                const decimal category3TransactionsAmount = (decimal)34.5;

                _mockTimePeriodHelper.Setup(helper => helper.ResolveDateRange(timePeriod)).Returns(dateRange);

                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory1Transactions, expectedCategory1,
                        category1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory2Transactions, expectedCategory2,
                        category2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndAmount(numberOfCategory3Transactions, expectedCategory3,
                        category3TransactionsAmount)
                    .Build();

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(It.IsAny<GetTransactionsQuery>()))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoriesBreakdown(expectedType, expectedCount, timePeriod);

                var expectedAnalyticsCategories = new List<AnalyticsCategory>
                {
                    new()
                    {
                        CategoryName = expectedCategory1,
                        TotalAmount = numberOfCategory1Transactions * category1TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory2,
                        TotalAmount = numberOfCategory2Transactions * category2TransactionsAmount
                    },
                    new()
                    {
                        CategoryName = expectedCategory3,
                        TotalAmount = numberOfCategory3Transactions * category3TransactionsAmount
                    }
                }.Take(expectedCount);

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }
        }

        public class GetSubcategoriesBreakdown
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetSubcategoriesBreakdown()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public async Task GivenInputCategoryNameAndDates_TransactionServiceCalledWithCorrectArguments()
            {
                const string expectedCategoryName = "category name";
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(
                            It.IsAny<GetTransactionsQuery>()
                        ))
                    .ReturnsAsync(() => new List<Transaction>
                    {
                        new Transaction
                        {
                            Amount = 123M,
                            Category = "category",
                            TransactionTimestamp = DateTime.Now.ToString("O"),
                            Subcategory = "subcategory",
                            TransactionId = "transaction-id-1",
                            TransactionType = "expense",
                        }
                    });

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                await service.GetSubcategoriesBreakdown(expectedCategoryName, null, start, end);

                _mockTransactionService.Verify(
                    transactionService => transactionService.GetTransactionsAsync(
                        It.Is<GetTransactionsQuery>(query =>
                            query.Categories.SequenceEqual(new List<string> { expectedCategoryName })
                            && query.Start == start && query.End == end
                        )));
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsSubcategoryListReturned()
            {
                const string expectedCategoryName = "category name";
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                const string expectedSubcategory1 = "subcategory1";
                const int numberOfSubcategory1Transactions = 24;
                const decimal subcategory1TransactionsAmount = (decimal)34.5;

                const string expectedSubcategory2 = "subcategory2";
                const int numberOfSubcategory2Transactions = 20;
                const decimal subcategory2TransactionsAmount = (decimal)34.5;

                const string expectedSubcategory3 = "subcategory3";
                const int numberOfSubcategory3Transactions = 10;
                const decimal subcategory3TransactionsAmount = (decimal)34.5;


                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory1Transactions,
                        expectedCategoryName, expectedSubcategory1,
                        subcategory1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory2Transactions,
                        expectedCategoryName, expectedSubcategory2,
                        subcategory2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory3Transactions,
                        expectedCategoryName, expectedSubcategory3,
                        subcategory3TransactionsAmount)
                    .Build();

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(
                            It.IsAny<GetTransactionsQuery>()
                        ))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories =
                    await service.GetSubcategoriesBreakdown(expectedCategoryName, null, start, end);

                var expectedAnalyticsCategories = new List<AnalyticsSubcategory>
                {
                    new()
                    {
                        SubcategoryName = expectedSubcategory1,
                        TotalAmount = numberOfSubcategory1Transactions * subcategory1TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    },
                    new()
                    {
                        SubcategoryName = expectedSubcategory2,
                        TotalAmount = numberOfSubcategory2Transactions * subcategory2TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    },
                    new()
                    {
                        SubcategoryName = expectedSubcategory3,
                        TotalAmount = numberOfSubcategory3Transactions * subcategory3TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    }
                };

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }

            [Fact]
            public async Task
                GivenInputCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsSubcategoryListReturned()
            {
                const int expectedCount = 2;
                const string expectedCategoryName = "category name";
                var start = DateTime.MinValue;
                var end = DateTime.MaxValue;

                const string expectedSubcategory1 = "subcategory1";
                const int numberOfSubcategory1Transactions = 24;
                const decimal subcategory1TransactionsAmount = (decimal)34.5;

                const string expectedSubcategory2 = "subcategory2";
                const int numberOfSubcategory2Transactions = 20;
                const decimal subcategory2TransactionsAmount = (decimal)34.5;

                const string expectedSubcategory3 = "subcategory3";
                const int numberOfSubcategory3Transactions = 10;
                const decimal subcategory3TransactionsAmount = (decimal)34.5;

                var transactionList = new TransactionListBuilder()
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory1Transactions,
                        expectedCategoryName, expectedSubcategory1,
                        subcategory1TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory2Transactions,
                        expectedCategoryName, expectedSubcategory2,
                        subcategory2TransactionsAmount)
                    .WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(numberOfSubcategory3Transactions,
                        expectedCategoryName, expectedSubcategory3,
                        subcategory3TransactionsAmount)
                    .Build();

                _mockTransactionService
                    .Setup(helperService =>
                        helperService.GetTransactionsAsync(
                            It.IsAny<GetTransactionsQuery>()
                        ))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories =
                    await service.GetSubcategoriesBreakdown(expectedCategoryName, expectedCount, start, end);

                var expectedAnalyticsCategories = new List<AnalyticsSubcategory>
                {
                    new()
                    {
                        SubcategoryName = expectedSubcategory1,
                        TotalAmount = numberOfSubcategory1Transactions * subcategory1TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    },
                    new()
                    {
                        SubcategoryName = expectedSubcategory2,
                        TotalAmount = numberOfSubcategory2Transactions * subcategory2TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    },
                    new()
                    {
                        SubcategoryName = expectedSubcategory3,
                        TotalAmount = numberOfSubcategory3Transactions * subcategory3TransactionsAmount,
                        BelongsToCategory = expectedCategoryName
                    }
                }.Take(expectedCount);

                Assert.Equal(expectedAnalyticsCategories, analyticsCategories);
            }
        }

        public class TransactionListBuilder
        {
            private readonly List<Transaction> _transactionList;

            public TransactionListBuilder()
            {
                _transactionList = new List<Transaction>();
            }

            public List<Transaction> Build()
            {
                return _transactionList;
            }

            public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndAmount(int number, string category,
                decimal amount)
            {
                for (var i = 0; i < number; i++)
                {
                    _transactionList.Add(new Transaction
                    {
                        Amount = amount,
                        Category = category,
                        TransactionTimestamp = DateTime.Now.ToString("O"),
                        Subcategory = "subcategory-1",
                        TransactionId = "transaction-id-1",
                        TransactionType = "expense",
                    });
                }

                return this;
            }

            public TransactionListBuilder WithNumberOfTransactionsOfCategoryAndSubcategoryAndAmount(int number,
                string category, string subcategory,
                decimal amount)
            {
                for (var i = 0; i < number; i++)
                {
                    _transactionList.Add(new Transaction
                    {
                        Amount = amount,
                        Category = category,
                        TransactionTimestamp = DateTime.Now.ToString("O"),
                        Subcategory = subcategory,
                        TransactionId = "transaction-id-1",
                        TransactionType = "expense",
                    });
                }

                return this;
            }
        }
    }
}