// Infrastructure/Repositories/DatabaseRepository.cs (Optimized)

using System.Data;
using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class DatabaseRepository(IConfiguration config) : IDatabaseRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");
    private readonly string? _masterConnectionString = config.GetConnectionString("MasterDatabase");

    public void CreateDatabase(string? dbName)
    {
        using var connection = CreateConnection(_masterConnectionString);
        var sql = $"CREATE DATABASE [{dbName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void DeleteDatabase(string? dbName)
    {
        using var connection = new SqlConnection(_masterConnectionString);
        connection.Open();

        try
        {
            // 1. بستن تمامی اتصالات فعال به دیتابیس
            const string killConnectionsSql = """

                                                              DECLARE @killSql NVARCHAR(MAX) = '';
                                                              SELECT @killSql = @killSql + 'KILL ' + CONVERT(NVARCHAR(10), session_id) + '; '
                                                              FROM sys.dm_exec_sessions
                                                              WHERE database_id = DB_ID(@DbName) AND session_id <> @@SPID;

                                                              EXEC sp_executesql @killSql;
                                                          
                                              """;

            using var killCommand = new SqlCommand(killConnectionsSql, connection);
            killCommand.Parameters.AddWithValue("@DbName", dbName);
            killCommand.ExecuteNonQuery();

            // 2. تغییر دیتابیس به حالت تک‌کاربره
            var setSingleUserSql = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
            using var singleUserCommand = new SqlCommand(setSingleUserSql, connection);
            singleUserCommand.ExecuteNonQuery();

            // 3. حذف دیتابیس
            var dropDatabaseSql = $"DROP DATABASE [{dbName}]";
            using var dropCommand = new SqlCommand(dropDatabaseSql, connection);
            dropCommand.ExecuteNonQuery();

            Console.WriteLine($"Database '{dbName}' deleted successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting database '{dbName}': {ex.Message}");
            throw;
        }
        finally
        {
            // اطمینان از بسته شدن اتصال
            if (connection.State == ConnectionState.Open) connection.Close();
        }
    }

    public void RenameDatabase(string? oldName, string? newName)
    {
        using var connection = new SqlConnection(_masterConnectionString);
        connection.Open();

        try
        {
            // 1. بستن تمامی اتصالات فعال به دیتابیس
            var killConnectionsSql = @"
                DECLARE @killSql NVARCHAR(MAX) = '';
                SELECT @killSql = @killSql + 'KILL ' + CONVERT(NVARCHAR(10), session_id) + '; '
                FROM sys.dm_exec_sessions
                WHERE database_id = DB_ID(@OldName) AND session_id <> @@SPID;

                EXEC sp_executesql @killSql;
            ";

            using var killCommand = new SqlCommand(killConnectionsSql, connection);
            killCommand.Parameters.AddWithValue("@OldName", oldName);
            killCommand.ExecuteNonQuery();

            // 2. تغییر دیتابیس به حالت تک‌کاربره (Single User)
            var setSingleUserSql = $"ALTER DATABASE [{oldName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
            using var singleUserCommand = new SqlCommand(setSingleUserSql, connection);
            singleUserCommand.ExecuteNonQuery();

            // 3. تغییر نام دیتابیس
            var renameSql = $"ALTER DATABASE [{oldName}] MODIFY NAME = [{newName}];";
            using var renameCommand = new SqlCommand(renameSql, connection);
            renameCommand.ExecuteNonQuery();

            // 4. بازگردانی به حالت چندکاربره (Multi User)
            var setMultiUserSql = $"ALTER DATABASE [{newName}] SET MULTI_USER;";
            using var multiUserCommand = new SqlCommand(setMultiUserSql, connection);
            multiUserCommand.ExecuteNonQuery();

            Console.WriteLine($"Database renamed successfully from '{oldName}' to '{newName}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming database: {ex.Message}");
            throw;
        }
        finally
        {
            // اطمینان از بسته شدن اتصال
            if (connection.State == ConnectionState.Open) connection.Close();
        }
    }

    public List<DatabaseModel> GetAllDatabases()
    {
        using var connection = CreateConnection(_masterConnectionString);
        const string sql = "SELECT name FROM sys.databases WHERE database_id > 4";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        var result = new List<DatabaseModel>();
        while (reader.Read()) result.Add(new DatabaseModel { Name = reader.GetString(0) });

        return result;
    }

    public DatabaseModel GetDatabaseSchema(string? dbName)
    {
        var databaseModel = new DatabaseModel { Name = dbName };
        var connectionString = _connectionStringTemplate?.Replace("{DBName}", dbName);

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // Fetch Database Description
        const string dbDescriptionQuery = """

                                                      SELECT CAST(ep.value AS NVARCHAR(MAX)) AS Description
                                                      FROM sys.extended_properties ep
                                                      WHERE ep.class = 0 AND ep.major_id = 0 AND ep.minor_id = 0 AND ep.name = 'MS_Description';
                                                  
                                          """;
        using (var dbDescCmd = new SqlCommand(dbDescriptionQuery, connection))
        {
            var result = dbDescCmd.ExecuteScalar();
            databaseModel.Description = result?.ToString() ?? "";
        }

        // Fetch Tables
        const string tableQuery = """

                                              SELECT 
                                                  t.name AS TableName,
                                                  CAST(ep.value AS NVARCHAR(MAX)) AS Description
                                              FROM sys.tables t
                                              LEFT JOIN sys.extended_properties ep 
                                                  ON t.object_id = ep.major_id 
                                                  AND ep.minor_id = 0 
                                                  AND ep.name = 'MS_Description'
                                              WHERE t.type = 'U'
                                              ORDER BY t.name;
                                          
                                  """;
        using (var tableCmd = new SqlCommand(tableQuery, connection))
        using (var reader = tableCmd.ExecuteReader())
        {
            while (reader.Read())
                databaseModel.Tables.Add(new TableModel
                {
                    Name = reader.GetString(0),
                    Description = reader.IsDBNull(1) ? "" : reader.GetString(1)
                });
        }

        // Fetch Views
        const string viewQuery = """

                                             SELECT 
                                                 v.name AS ViewName,
                                                 CAST(ep.value AS NVARCHAR(MAX)) AS Description
                                             FROM sys.views v
                                             LEFT JOIN sys.extended_properties ep 
                                                 ON v.object_id = ep.major_id 
                                                 AND ep.minor_id = 0 
                                                 AND ep.name = 'MS_Description'
                                             ORDER BY v.name;
                                         
                                 """;
        using (var viewCmd = new SqlCommand(viewQuery, connection))
        using (var reader = viewCmd.ExecuteReader())
        {
            while (reader.Read())
                databaseModel.Views.Add(new ViewModel
                {
                    Name = reader.GetString(0),
                    Description = reader.IsDBNull(1) ? "" : reader.GetString(1)
                });
        }

        // Fetch Stored Procedures
        const string spQuery = """

                                           SELECT 
                                               p.name AS ProcedureName,
                                               CAST(ep.value AS NVARCHAR(MAX)) AS Description
                                           FROM sys.procedures p
                                           LEFT JOIN sys.extended_properties ep 
                                               ON p.object_id = ep.major_id 
                                               AND ep.minor_id = 0 
                                               AND ep.name = 'MS_Description'
                                           ORDER BY p.name;
                                       
                               """;
        using (var spCmd = new SqlCommand(spQuery, connection))
        using (var reader = spCmd.ExecuteReader())
        {
            while (reader.Read())
                databaseModel.StoredProcedures.Add(new StoredProcedureModel
                {
                    Name = reader.GetString(0),
                    Description = reader.IsDBNull(1) ? "" : reader.GetString(1)
                });
        }

        // Fetch Functions
        const string functionQuery = """

                                                 SELECT 
                                                     o.name AS FunctionName,
                                                     CAST(ep.value AS NVARCHAR(MAX)) AS Description,
                                                     o.type AS FunctionType
                                                 FROM sys.objects o
                                                 LEFT JOIN sys.extended_properties ep 
                                                     ON o.object_id = ep.major_id 
                                                     AND ep.minor_id = 0 
                                                     AND ep.name = 'MS_Description'
                                                 WHERE o.type IN ('FN', 'IF', 'TF')
                                                 ORDER BY o.name;
                                             
                                     """;
        using (var funcCmd = new SqlCommand(functionQuery, connection))
        using (var reader = funcCmd.ExecuteReader())
        {
            while (reader.Read())
            {
                var functionTypeCode = reader.GetString(2);
                var functionType = functionTypeCode switch
                {
                    "FN" => FunctionType.Scalar,
                    "IF" => FunctionType.InlineTableValued,
                    "TF" => FunctionType.MultiStatementTableValued,
                    _ => FunctionType.Scalar
                };

                databaseModel.Functions.Add(new FunctionModel
                {
                    Name = reader.GetString(0),
                    Description = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Type = functionType
                });
            }
        }

        return databaseModel;
    }


    public bool DatabaseExists(string? dbName)
    {
        using var connection = CreateConnection(_masterConnectionString);
        const string sql = "SELECT COUNT(*) FROM sys.databases WHERE name = @dbName";
        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@dbName", dbName);
        var count = (int)command.ExecuteScalar();
        return count > 0;
    }

    public void SetDatabaseDescription(string? dbName, string description)
    {
        var connectionString = _connectionStringTemplate?.Replace("{DBName}", dbName);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string is not configured properly.");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        // بررسی وجود ویژگی
        const string checkSql = """

                                        SELECT 1 
                                        FROM sys.extended_properties 
                                        WHERE name = N'MS_Description' 
                                          AND class = 0 
                                          AND major_id = 0
                                """;

        using var checkCommand = new SqlCommand(checkSql, connection);
        var exists = checkCommand.ExecuteScalar() != null;

        var sql =
            // به‌روزرسانی ویژگی موجود
            exists
                ? """

                              EXEC sp_updateextendedproperty 
                                  @name = N'MS_Description', 
                                  @value = @description;
                  """
                :
                // افزودن ویژگی جدید
                """

                            EXEC sp_addextendedproperty 
                                @name = N'MS_Description', 
                                @value = @description;
                """;

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@description", description);
        command.ExecuteNonQuery();
    }

    public string GetDatabaseDescription(string? dbName)
    {
        var connectionString = _connectionStringTemplate?.Replace("{DBName}", dbName);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string is not configured properly.");

        using var connection = new SqlConnection(connectionString);
        connection.Open();

        const string sql = """

                                   SELECT CAST(value AS NVARCHAR(MAX)) 
                                   FROM sys.extended_properties 
                                   WHERE class = 0 AND major_id = 0 AND name = 'MS_Description'
                           """;

        using var command = new SqlCommand(sql, connection);
        var result = command.ExecuteScalar();

        return result?.ToString() ?? string.Empty;
    }

    private static SqlConnection CreateConnection(string? connectionString)
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }
}