// Infrastructure/Repositories/ViewRepository.cs

using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class ViewRepository(IConfiguration config) : IViewRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");

    public void CreateView(string dbName, ViewModel view)
    {
        using var connection = GetConnection(dbName);
        var sql = $"CREATE VIEW [{view.Name}] AS {view.DefinitionSql}";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void DeleteView(string dbName, string viewName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"DROP VIEW [{viewName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public ViewModel GetView(string dbName, string viewName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"SELECT definition FROM sys.sql_modules WHERE object_id = OBJECT_ID('[dbo].[{viewName}]')";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        if (reader.Read()) return new ViewModel { Name = viewName, DefinitionSql = reader.GetString(0) };
        throw new Exception($"View '{viewName}' not found.");
    }

    public List<ViewModel> GetAllViews(string dbName)
    {
        using var connection = GetConnection(dbName);
        var sql = "SELECT name FROM sys.views ORDER BY name";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        var views = new List<ViewModel>();
        while (reader.Read()) views.Add(new ViewModel { Name = reader.GetString(0) });
        return views;
    }

    public void SetViewDescription(string dbName, string viewName, string description)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"EXEC sp_addextendedproperty @name=N'MS_Description', @value=N'{description}', @level0type=N'Schema', @level0name=N'dbo', @level1type=N'View', @level1name=N'{viewName}'";
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