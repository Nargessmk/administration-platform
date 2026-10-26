// Infrastructure/Repositories/StoredProcedureRepository.cs

using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class StoredProcedureRepository(IConfiguration config) : IStoredProcedureRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");

    public void CreateStoredProcedure(string dbName, StoredProcedureModel sp)
    {
        using var connection = GetConnection(dbName);
        var sql = sp.DefinitionSql;
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void DeleteStoredProcedure(string dbName, string spName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"DROP PROCEDURE [{spName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public StoredProcedureModel GetStoredProcedure(string dbName, string spName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"SELECT definition FROM sys.sql_modules WHERE object_id = OBJECT_ID('[dbo].[{spName}]')";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        if (reader.Read()) return new StoredProcedureModel { Name = spName, DefinitionSql = reader.GetString(0) };
        throw new Exception($"Stored procedure '{spName}' not found.");
    }

    public List<StoredProcedureModel> GetAllStoredProcedures(string dbName)
    {
        using var connection = GetConnection(dbName);
        const string sql = "SELECT name FROM sys.procedures ORDER BY name";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        var procedures = new List<StoredProcedureModel>();
        while (reader.Read()) procedures.Add(new StoredProcedureModel { Name = reader.GetString(0) });
        return procedures;
    }

    public void SetStoredProcedureDescription(string dbName, string spName, string description)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"EXEC sp_addextendedproperty @name=N'MS_Description', @value=N'{description}', @level0type=N'Schema', @level0name=N'dbo', @level1type=N'Procedure', @level1name=N'{spName}'";
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