using System.Data;
using Npgsql;

namespace TransactionService.Repositories.CockroachDb
{
    public class DapperContext
    {
        private string _connectionString { get; init; }

        public DapperContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}