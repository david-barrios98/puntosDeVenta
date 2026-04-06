using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class SqlConfigServer
{
    protected readonly string ConnectionString;

    public SqlConfigServer(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("DefaultConnection");
    }

    protected async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    protected async Task<List<T>> ExecuteStoredProcedureAsync<T>(
        string storedProcedureName,
        SqlParameter[] parameters,
        Func<SqlDataReader, T> mapFunction)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        var results = new List<T>();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(mapFunction(reader));
        }

        return results;
    }

    protected async Task<T?> ExecuteStoredProcedureSingleAsync<T>(
        string storedProcedureName,
        SqlParameter[] parameters,
        Func<SqlDataReader, T> mapFunction)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return mapFunction(reader);
        }

        return default; // null si no encuentra nada
    }

    protected SqlParameter CreateParameter(string name, object value, SqlDbType dbType)
    {
        return new SqlParameter(name, dbType) { Value = value ?? DBNull.Value };
    }

    protected async Task ExecuteInTransactionAsync(Func<SqlTransaction, Task> operation)
    {
        using var connection = await OpenConnectionAsync();
        using var transaction = connection.BeginTransaction();
        try
        {
            await operation(transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    protected async Task<int> ExecuteStoredProcedureNonQueryAsync(
        string storedProcedureName,
        SqlParameter[] parameters)
    {
        using var connection = await OpenConnectionAsync();
        using var command = new SqlCommand(storedProcedureName, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        return await command.ExecuteNonQueryAsync();
    }

    protected SqlParameter CreateOutputParameter(string name, SqlDbType dbType)
    {
        return new SqlParameter(name, dbType)
        {
            Direction = ParameterDirection.Output
        };
    }
}

