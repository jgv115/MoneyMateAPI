using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using MoneyMateApi.Domain.Transactions.Specifications;
using MoneyMateApi.Helpers.TimePeriodHelpers;
using MoneyMateApi.Middleware;
using MoneyMateApi.Repositories.CockroachDb.Entities;

namespace MoneyMateApi.Repositories.CockroachDb
{
    public static class TransactionDapperHelpers
    {
        public static async Task<IEnumerable<Transaction>> QueryAndBuildTransactions(IDbConnection connection,
            string query, object parameters = null)
        {
            var transactionDictionary = new Dictionary<Guid, Transaction>();

            var transactions =
                await connection
                    .QueryAsync<Transaction, TransactionType?, Category, Subcategory, PayerPayee?, Guid?, Transaction>(
                        query,
                        (
                            transaction, transactionType, category, subcategory, payerPayee, tagId) =>
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

                            if (payerPayee != null && payerPayee.Id != Guid.Empty)
                            {
                                accumulatedTransaction.PayerPayeeId = payerPayee.Id;
                                accumulatedTransaction.PayerPayeeName = payerPayee.Name;
                            }

                            if (tagId != null && tagId != Guid.Empty)
                            {
                                accumulatedTransaction.TagIds.Add(tagId.Value);
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

        public async Task<Domain.Transactions.Transaction> GetTransactionById(string transactionId)
        {
            var query =
                @"SELECT transaction.id,
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
                        pp.external_link_id as externalLinkId,

                        ttags.tag_id as id
                 FROM transaction
                         LEFT JOIN transactiontype tt on transaction.transaction_type_id = tt.id
                         LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id 
                         LEFT JOIN category c on sc.category_id = c.id
                         LEFT JOIN payerpayee pp on transaction.payerpayee_id = pp.id
                         LEFT JOIN transactiontags ttags on transaction.id = ttags.transaction_id
                 WHERE transaction.profile_id = @profile_id and transaction.id = @transactionId
                 ";

            using (var connection = _context.CreateConnection())
            {
                var transactions = await TransactionDapperHelpers.QueryAndBuildTransactions(connection, query,
                    new { profile_id = _userContext.ProfileId, transactionId });

                return _mapper.Map<Transaction, Domain.Transactions.Transaction>(
                    transactions.FirstOrDefault((Transaction)null));
            }
        }

        public async Task<IEnumerable<Domain.Transactions.Transaction>> GetTransactions(DateRange dateRange,
            ITransactionSpecification spec)
        {
            var query =
                @"SELECT transaction.id,
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
                        pp.external_link_id as externalLinkId,
                        
                        ttags.tag_id as id
                 FROM transaction
                         LEFT JOIN transactiontype tt on transaction.transaction_type_id = tt.id
                     
                         LEFT JOIN subcategory sc on transaction.subcategory_id = sc.id 
                         LEFT JOIN category c on sc.category_id = c.id
                     
                         LEFT JOIN payerpayee pp on transaction.payerpayee_id = pp.id
                     
                         LEFT JOIN transactiontags ttags on transaction.id = ttags.transaction_id
                 WHERE transaction.profile_id = @profile_id
                   AND transaction_timestamp >= @start_timestamp AND transaction_timestamp < @end_timestamp
                ORDER BY transaction.transaction_timestamp
                 ";

            using (var connection = _context.CreateConnection())
            {
                var transactions = await TransactionDapperHelpers.QueryAndBuildTransactions(connection, query,
                    new
                    {
                        profile_id = _userContext.ProfileId,
                        start_timestamp = dateRange.Start,
                        end_timestamp =
                            DateTime.MaxValue == dateRange.End
                                ? new DateTime(9999, 12, 31, 23, 59, 59, 999)
                                : dateRange.End
                    });

                var mappedTransactions =
                    _mapper.Map<IEnumerable<Transaction>, IEnumerable<Domain.Transactions.Transaction>>(transactions);

                return mappedTransactions.Where(spec.IsSatisfied).ToList();
            }
        }

        public async Task StoreTransaction(Domain.Transactions.Transaction newTransaction)
        {
            const string insertTransactionQuery = @"
                WITH ins (transaction_id, user_identifier, timestamp, transaction_type, amount, category, subcategory, payerpayeeid, notes, profile_id) AS
                         (VALUES (@transaction_id, @user_identifier, @transaction_timestamp, @transaction_type, @amount, @category, @subcategory,
                                  @payerpayeeid, @notes, @profile_id))
                UPSERT
                INTO transaction (id, user_id, transaction_timestamp, transaction_type_id, amount, subcategory_id, payerPayee_id, notes, profile_id)
                SELECT ins.transaction_id,
                       u.id,
                       ins.timestamp,
                       tt.id,
                       ins.amount,
                       cs.SUBCATEGORYID,
                       ins.payerpayeeid,
                       ins.notes,
                       ins.profile_id
                FROM ins
                         JOIN users u ON u.user_identifier = ins.user_identifier
                         JOIN transactiontype tt ON tt.name = ins.transaction_type
                         JOIN categories_and_subcategories cs
                              on cs.profileid = ins.profile_id AND SUBCATEGORYNAME = ins.subcategory AND
                                 categoryname = ins.category;
            ";

            const string insertTransactionTagsQuery = @"
                INSERT INTO transactiontags (transaction_id, tag_id)
                VALUES (@transaction_id, @tag_id)";

            using (var connection = _context.CreateConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    var transactionId = Guid.Parse(newTransaction.TransactionId);
                    await connection.ExecuteAsync(insertTransactionQuery, new
                    {
                        transaction_id = transactionId,
                        user_identifier = _userContext.UserId,
                        transaction_timestamp = DateTimeOffset.Parse(newTransaction.TransactionTimestamp),
                        transaction_type = newTransaction.TransactionType,
                        amount = newTransaction.Amount,
                        category = newTransaction.Category,
                        subcategory = newTransaction.Subcategory,
                        payerpayeeid = Guid.TryParse(newTransaction.PayerPayeeId, out var payerPayeeId)
                            ? payerPayeeId
                            : (Guid?)null,
                        notes = newTransaction.Note,
                        profile_id = _userContext.ProfileId
                    });


                    await connection.ExecuteAsync(insertTransactionTagsQuery, newTransaction.TagIds.Select(tagId => new
                    {
                        transaction_id = transactionId,
                        tag_id = tagId
                    }));

                    transaction.Commit();
                }
            }
        }

        public Task PutTransaction(Domain.Transactions.Transaction newTransaction)
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