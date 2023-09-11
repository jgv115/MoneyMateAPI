using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using TransactionService.Domain.Services.Transactions.Specifications;
using TransactionService.Helpers.TimePeriodHelpers;
using TransactionService.Middleware;
using TransactionService.Repositories.CockroachDb.Entities;

namespace TransactionService.Repositories.CockroachDb
{
    public static class TransactionDapperHelpers
    {
        public static async Task<IEnumerable<Transaction>> QueryAndBuildTransactions(IDbConnection connection,
            string query, object parameters = null)
        {
            var transactionDictionary = new Dictionary<Guid, Transaction>();

            var transactions =
                await connection
                    .QueryAsync<Transaction, TransactionType, Category, Subcategory, PayerPayee, Transaction>(query,
                        (
                            transaction, transactionType, category, subcategory, payerPayee) =>
                        {
                            Transaction accumulatedTransaction;

                            if (!transactionDictionary.TryGetValue(transaction.Id, out accumulatedTransaction))
                            {
                                accumulatedTransaction = transaction;
                                transactionDictionary.Add(transaction.Id, accumulatedTransaction);
                            }

                            if (transactionType != null)
                                accumulatedTransaction.TransactionType = transactionType.Name;

                            if (category != null)
                                accumulatedTransaction.Category = category.Name;

                            if (subcategory != null)
                                accumulatedTransaction.Subcategory = subcategory.Name;

                            // PayerPayee is nullable
                            if (payerPayee != null && payerPayee.Id != Guid.Empty)
                            {
                                accumulatedTransaction.PayerPayeeId = payerPayee.Id;
                                accumulatedTransaction.PayerPayeeName = payerPayee.Name;
                            }

                            return accumulatedTransaction;
                        }, parameters);

            return transactions.Distinct();
        }
    }

    public class CockroachDbTransactionRepository : ITransactionRepository
    {
        private readonly DapperContext _context;
        private readonly IMapper _mapper;
        private readonly CurrentUserContext _userContext;

        public CockroachDbTransactionRepository(DapperContext context, IMapper mapper, CurrentUserContext userContext)
        {
            _context = context;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<Domain.Models.Transaction> GetTransactionById(string transactionId)
        {
            var query =
                @"SELECT transaction.id,
                        u.id           as userId,
                        transaction.transaction_timestamp as transactionTimestamp,
                        transaction.amount,
                        transaction.notes as note,

                        tt.id,
                        tt.name as name,
                        
                        c.id,
                        c.name as name,
                        
                        sc.id,
                        sc.name as name,
                        
                        pp.id,
                        pp.name             as name,
                        pp.external_link_id as externalLinkId
                 FROM transaction
                         JOIN users u on transaction.user_id = u.id
                         LEFT JOIN transactiontype tt on transaction.transaction_type_id = tt.id
                         LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id 
                         LEFT JOIN category c on sc.category_id = c.id
                         LEFT JOIN payerpayee pp on transaction.payerpayee_id = pp.id
                 WHERE u.user_identifier = @user_identifier and transaction.id = @transactionId
                 ";

            using (var connection = _context.CreateConnection())
            {
                var transactions = await TransactionDapperHelpers.QueryAndBuildTransactions(connection, query,
                    new {user_identifier = _userContext.UserId, transactionId});

                return _mapper.Map<Transaction, Domain.Models.Transaction>(
                    transactions.FirstOrDefault((Transaction) null));
            }
        }

        public async Task<List<Domain.Models.Transaction>> GetTransactions(DateRange dateRange,
            ITransactionSpecification spec)
        {
            var query =
                @"SELECT transaction.id,
                        u.id           as userId,
                        transaction.transaction_timestamp as transactionTimestamp,
                        transaction.amount,
                        transaction.notes as note,

                        tt.id,
                        tt.name as name,
                        
                        c.id,
                        c.name as name,
                        
                        sc.id,
                        sc.name as name,
                        
                        pp.id,
                        pp.name             as name,
                        pp.external_link_id as externalLinkId
                 FROM transaction
                         JOIN users u on transaction.user_id = u.id
                         LEFT JOIN transactiontype tt on transaction.transaction_type_id = tt.id
                         LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id 
                         LEFT JOIN category c on sc.category_id = c.id
                         LEFT JOIN payerpayee pp on transaction.payerpayee_id = pp.id
                 WHERE u.user_identifier = @user_identifier
                   AND transaction_timestamp > @start_timestamp AND transaction_timestamp < @end_timestamp
                ORDER BY transaction.transaction_timestamp
                 ";

            using (var connection = _context.CreateConnection())
            {
                var transactions = await TransactionDapperHelpers.QueryAndBuildTransactions(connection, query,
                    new
                    {
                        user_identifier = _userContext.UserId,
                        start_timestamp = dateRange.Start,
                        end_timestamp =
                            DateTime.MaxValue == dateRange.End
                                ? new DateTime(9999, 12, 31, 23, 59, 59, 999)
                                : dateRange.End
                    });

                var mappedTransactions =
                    _mapper.Map<List<Transaction>, List<Domain.Models.Transaction>>(transactions.ToList());

                return mappedTransactions.Where(transaction => spec.IsSatisfied(transaction)).ToList();
            }
        }

        public async Task StoreTransaction(Domain.Models.Transaction newTransaction)
        {
            var query = @"
                WITH ins (transaction_id, user_identifier, timestamp, transaction_type, amount, subcategory, payerpayeeid, notes) AS
                         (VALUES (@transaction_id, @user_identifier, @transaction_timestamp, @transaction_type, @amount, @subcategory,
                                  @payerpayeeid, @notes)),
                     userRow as (SELECT id
                                 FROM users
                                 where user_identifier = (SELECT ins.user_identifier from ins))
                UPSERT
                INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerPayee_id, notes)
                SELECT ins.transaction_id,
                       u.id,
                       ins.timestamp,
                       tt.id,
                       ins.amount,
                       cs.SUBCATEGORYID,
                       ins.payerpayeeid,
                       ins.notes
                FROM ins
                         JOIN users u ON u.user_identifier = ins.user_identifier
                         JOIN transactiontype tt ON tt.name = ins.transaction_type
                         JOIN categories_and_subcategories cs
                              on u.user_identifier = cs.user_identifier AND SUBCATEGORYNAME = ins.subcategory;
            ";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new
                {
                    transaction_id = Guid.Parse(newTransaction.TransactionId),
                    user_identifier = _userContext.UserId,
                    transaction_timestamp = DateTimeOffset.Parse(newTransaction.TransactionTimestamp),
                    transaction_type = newTransaction.TransactionType,
                    amount = newTransaction.Amount,
                    subcategory = newTransaction.Subcategory,
                    payerpayeeid = Guid.TryParse(newTransaction.PayerPayeeId, out var payerPayeeId)
                        ? payerPayeeId
                        : (Guid?) null,
                    notes = newTransaction.Note
                });
            }
        }

        public Task PutTransaction(Domain.Models.Transaction newTransaction)
        {
            return StoreTransaction(newTransaction);
        }

        public async Task DeleteTransaction(string transactionId)
        {
            var query = @"DELETE FROM transaction WHERE id = @transaction_id";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, new
                {
                    transaction_id = transactionId
                });
            }
        }
    }
}