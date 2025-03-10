using System.Data;
using Npgsql;

namespace MoneyMateApi.Repositories.CockroachDb
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