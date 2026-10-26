// Infrastructure/Repositories/FunctionRepository.cs

using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class FunctionRepository(IConfiguration config) : IFunctionRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");

    public void CreateFunction(string dbName, FunctionModel function)
    {
        using var connection = GetConnection(dbName);
        var sql = function.DefinitionSql;
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void DeleteFunction(string dbName, string functionName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"DROP FUNCTION [{functionName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public FunctionModel GetFunction(string dbName, string functionName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"SELECT definition FROM sys.sql_modules WHERE object_id = OBJECT_ID('[dbo].[{functionName}]')";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        if (reader.Read()) return new FunctionModel { Name = functionName, DefinitionSql = reader.GetString(0) };
        throw new Exception($"Function '{functionName}' not found.");
    }

    public List<FunctionModel> GetAllFunctions(string dbName)
    {
        using var connection = GetConnection(dbName);
        var sql = "SELECT name FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') ORDER BY name";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        var functions = new List<FunctionModel>();
        while (reader.Read())
            functions.Add(new FunctionModel { Name = reader.GetString(0), Type = FunctionType.Scalar });
        return functions;
    }

    public void SetFunctionDescription(string dbName, string functionName, string description)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"EXEC sp_addextendedproperty @name=N'MS_Description', @value=N'{description}', @level0type=N'Schema', @level0name=N'dbo', @level1type=N'Function', @level1name=N'{functionName}'";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    private SqlConnection GetConnection(string dbName)
    {
        var connStr = _connectionStringTemplate?.Replace("{DBName}", dbName);
        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }
}