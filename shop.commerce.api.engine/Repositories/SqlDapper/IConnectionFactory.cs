using System.Data;

namespace shop.commerce.api.infrastructure.Repositories.SqlDapper
{
    public interface IConnectionFactory
    {
        IDbConnection GetConnection(string connectionString, int connectionTimeout);
    }
}