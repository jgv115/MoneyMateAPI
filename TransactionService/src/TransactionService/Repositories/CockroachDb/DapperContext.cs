using System.Data;
using Npgsql;

namespace TransactionService.Repositories.CockroachDb
{
    public class DapperContext
    {
        private string ConnectionString { get; init; }

        public DapperContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(ConnectionString);
    }
}