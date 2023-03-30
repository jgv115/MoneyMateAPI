using System.Collections.Generic;
using TransactionService.Constants;
using TransactionService.Controllers.Transactions.Dtos;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using Xunit;

namespace TransactionService.Tests.Domain.Specifications;

public class TransactionSpecificationFactoryTests
{
    public class Create
    {
        [Fact]
        public void GivenNoFilterParametersInQuery_ThenEmptyAndSpecificationReturned()
        {
            var factory = new TransactionSpecificationFactory();
            var returnedSpec = factory.Create(new GetTransactionsQuery());


            Assert.True(returnedSpec.IsSatisfied(new Transaction()));
        }

        [Fact]
        public void GivenTransactionTypeInQuery_ThenSpecThatFiltersByTypeIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                Type = TransactionType.Expense
            });


            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "income"
            }));
        }
        
        [Fact]
        public void GivenCategoriesInQuery_ThenSpecThatFiltersByCategoriesIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                Categories = new List<string> {"category1", "category2"}
            });


            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                Category = "category1"
            }));
            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                Category = "category2"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                Category = "invalid category"
            }));
        }
        
        [Fact]
        public void GivenSubcategoriesInQuery_ThenSpecThatFiltersBySubcategoriesIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            });


            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                Subcategory = "subcategory1"
            }));
            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                Subcategory = "subcategory2"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                Subcategory = "invalid subcategory"
            }));
        }
        
        [Fact]
        public void GivenPayerPayeeIdsInQuery_ThenSpecThatFiltersByPayerPayeeIdsIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                PayerPayeeIds = new List<string> {"payer1", "payer2"}
            });

            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                PayerPayeeId = "payer1"
            }));
            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                PayerPayeeId = "payer2"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                PayerPayeeId = "payer3"
            }));
        }

        
        [Fact]
        public void GivenTypeAndCategoriesInQuery_ThenSpecThatFiltersByTypeAndCategoriesIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                Type = TransactionType.Expense,
                Categories = new List<string> {"category1", "category2"}
            });


            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Category = "category1"
            }));
            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Category = "category2"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Category = "invalid subcategory"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "income",
                Category = "category1"
            }));
        }
        
        [Fact]
        public void GivenTypeAndSubcategoriesInQuery_ThenSpecThatFiltersByTypeAndSubcategoriesIsReturned()
        {
            var factory = new TransactionSpecificationFactory();

            var returnedSpec = factory.Create(new GetTransactionsQuery
            {
                Type = TransactionType.Expense,
                Subcategories = new List<string> {"subcategory1", "subcategory2"}
            });


            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Subcategory = "subcategory1"
            }));
            Assert.True(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Subcategory = "subcategory2"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "expense",
                Subcategory = "invalid subcategory"
            }));
            Assert.False(returnedSpec.IsSatisfied(new Transaction
            {
                TransactionType = "income",
                Subcategory = "subcategory1"
            }));
        }
    }
}