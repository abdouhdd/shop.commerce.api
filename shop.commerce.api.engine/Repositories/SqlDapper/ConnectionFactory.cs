using System.Data;
using Microsoft.Data.SqlClient;

namespace shop.commerce.api.infrastructure.Repositories.SqlDapper
{
    public class ConnectionFactory : IConnectionFactory
    {
        private IDbConnection _connection;

        public ConnectionFactory()
        {
        }

        public IDbConnection GetConnection(string connectionString, int connectionTimeout) 
        {
            _connection = new SqlConnection(connectionString)
            {
                ConnectionString = connectionString
            };
            //_connection.ConnectionString = connectionString;
            //_connection.CommandTimeout = connectionTimeout;
            return _connection;
        }
    }
}
