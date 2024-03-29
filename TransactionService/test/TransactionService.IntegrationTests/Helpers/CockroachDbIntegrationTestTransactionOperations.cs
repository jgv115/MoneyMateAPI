using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using TransactionService.Constants;
using TransactionService.Domain.Models;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Repositories;
using TransactionService.Repositories.CockroachDb;

namespace TransactionService.IntegrationTests.Helpers;

public class CockroachDbIntegrationTestTransactionOperations
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly CockroachDbIntegrationTestTransactionTypeOperations _transactionTypeOperations;
    private readonly CockroachDbIntegrationTestCategoryOperations _categoryOperations;
    private readonly CockroachDbIntegrationTestPayerPayeeOperations _payerPayeeOperations;

    private readonly Guid _testUserId;
    private readonly DapperContext _dapperContext;

    public CockroachDbIntegrationTestTransactionOperations(DapperContext dapperContext,
        ITransactionRepository transactionRepository,
        Guid testUserId,
        CockroachDbIntegrationTestTransactionTypeOperations transactionTypeOperations,
        CockroachDbIntegrationTestCategoryOperations categoryOperations,
        CockroachDbIntegrationTestPayerPayeeOperations payerPayeeOperations)
    {
        _transactionRepository = transactionRepository;
        _testUserId = testUserId;
        _transactionTypeOperations = transactionTypeOperations;
        _categoryOperations = categoryOperations;
        _payerPayeeOperations = payerPayeeOperations;
        _dapperContext = dapperContext;
    }


    public async Task<Transaction> GetTransactionById(string transactionId)
    {
        return await _transactionRepository.GetTransactionById(transactionId);
    }

    public async Task<List<Transaction>> GetAllTransactions()
    {
        return await _transactionRepository.GetTransactions(new DateRange(DateTime.MinValue, DateTime.MaxValue),
            new AndSpec(new List<ITransactionSpecification>()));
    }


    public async Task WriteTransactionsIntoDb(List<Transaction> transactions)
    {
        var transactionTypeIds = _transactionTypeOperations.GetTransactionTypeIds();
        using (var connection = _dapperContext.CreateConnection())
        {
            var createTransactionQuery = @"
                INSERT INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerpayee_id, notes, profile_id)
                VALUES (@transactionId, @userId, @timestamp, @transactionTypeId, @amount, @subcategoryId, @payerpayeeId, @notes, @profileId)
            ";

            foreach (var transaction in transactions)
            {
                var parameters = new DynamicParameters();
                parameters.Add("transactionId", transaction.TransactionId);
                parameters.Add("userId", _testUserId);
                parameters.Add("profileId", _testUserId);
                parameters.Add("timestamp", transaction.TransactionTimestamp);
                parameters.Add("transactionTypeId",
                    transaction.TransactionType == TransactionType.Expense.ToProperString()
                        ? transactionTypeIds.Expense
                        : transactionTypeIds.Income);
                parameters.Add("amount", transaction.Amount);
                parameters.Add("notes", transaction.Note);

                TransactionType parsedTransactionType;
                Enum.TryParse(transaction.TransactionType, out parsedTransactionType);
                var categoryId = await _categoryOperations.WriteCategoryIntoDb(connection, new Category
                {
                    TransactionType = parsedTransactionType,
                    CategoryName = transaction.Category
                }, transactionTypeIds);

                var subcategoryId =
                    await _categoryOperations.WriteSubcategoryIntoDb(connection, categoryId, transaction.Subcategory);
                parameters.Add("subcategoryId", subcategoryId);

                if (string.IsNullOrEmpty(transaction.PayerPayeeId))
                    parameters.Add("payerPayeeId");
                else
                {
                    var payerPayeeId = await _payerPayeeOperations.WritePayerPayeeIntoDb(
                        new PayerPayee
                        {
                            PayerPayeeId = transaction.PayerPayeeId,
                            PayerPayeeName = transaction.PayerPayeeName,
                            ExternalId = ""
                        }, transaction.TransactionType == TransactionType.Expense.ToProperString() ? "payee" : "payer");
                    parameters.Add("payerPayeeId", payerPayeeId);
                }

                await connection.ExecuteAsync(createTransactionQuery, parameters);
            }
        }
    }
}