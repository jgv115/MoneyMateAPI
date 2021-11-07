using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services;
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

        public class GetCategoryBreakdown_ByDateTime
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetCategoryBreakdown_ByDateTime()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                const string expectedType = "expense";
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
                        helperService.GetAllTransactionsAsync(start, end,
                            expectedType)).ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoryBreakdown(expectedType, null, start, end);

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
                const string expectedType = "expense";
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
                        helperService.GetAllTransactionsAsync(start, end,
                            expectedType)).ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoryBreakdown(expectedType, expectedCount, start, end);

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

        public class GetCategoryBreakdown_ByTimePeriod
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetCategoryBreakdown_ByTimePeriod()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_ThenCorrectAnalyticsCategoryListReturned()
            {
                const string expectedType = "expense";
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
                        helperService.GetAllTransactionsAsync(start, end,
                            expectedType)).ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoryBreakdown(expectedType, null, timePeriod);

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
                const string expectedType = "expense";
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
                        helperService.GetAllTransactionsAsync(start, end,
                            expectedType)).ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetCategoryBreakdown(expectedType, expectedCount, timePeriod);

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

        public class GetSubCategoriesBreakdown
        {
            private readonly Mock<ITransactionHelperService> _mockTransactionService;
            private readonly Mock<ITimePeriodHelper> _mockTimePeriodHelper;

            public GetSubCategoriesBreakdown()
            {
                _mockTransactionService = new Mock<ITransactionHelperService>();
                _mockTimePeriodHelper = new Mock<ITimePeriodHelper>();
            }

            [Fact]
            public async Task
                GivenNullCountAndListReturnedFromTransactionService_WhenGetSubcategoriesBreakdownInvoked_ThenCorrectAnalyticsSubcategoryListReturned()
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
                        helperService.GetAllTransactionsByCategoryAsync(expectedCategoryName, start, end))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetSubcategoriesBreakdown(expectedCategoryName, null, start, end);

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
                GivenInputCountAndListReturnedFromTransactionService_WhenGetSubcategoriesBreakdownInvoked_ThenCorrectAnalyticsSubcategoryListReturned()
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
                        helperService.GetAllTransactionsByCategoryAsync(expectedCategoryName, start, end))
                    .ReturnsAsync(() => transactionList);

                var service = new AnalyticsService(_mockTransactionService.Object, _mockTimePeriodHelper.Object);
                var analyticsCategories = await service.GetSubcategoriesBreakdown(expectedCategoryName, expectedCount, start, end);

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
                        SubCategory = "subcategory-1",
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
                        SubCategory = subcategory,
                        TransactionId = "transaction-id-1",
                        TransactionType = "expense",
                    });
                }

                return this;
            }
        }
    }
}