using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace __RootNamespace__.Infrastructure.Context;

public class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "No se encontró 'DefaultConnection'.");
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
