using System.Text.RegularExpressions;
using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class TriggerRepository(IConfiguration config) : ITriggerRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");

    public void CreateTrigger(string dbName, string tableName, TriggerModel trigger)
    {
        using var connection = GetConnection(dbName);
        // 1. Check if the trigger exists
        var triggerExistsSql = $"""

                                        SELECT 1 
                                        FROM sys.triggers 
                                        WHERE name = '{trigger.TriggerName?.Replace("'", "''")}' 
                                        AND parent_id = OBJECT_ID('{tableName.Replace("'", "''")}')
                                """;

        bool triggerExists;
        using (var existsCommand = new SqlCommand(triggerExistsSql, connection))
        {
            triggerExists = existsCommand.ExecuteScalar() != null;
        }

        // 2. Create or Alter the trigger
        var triggerSql = triggerExists
            ? $"""
               ALTER TRIGGER [{trigger.TriggerName}]
                            ON [{tableName}]
                            {trigger.ActionType} {trigger.Action}
                            AS
                            BEGIN
                                SET NOCOUNT ON;
                                {trigger.Query}
                            END
               """
            : $@"CREATE TRIGGER [{trigger.TriggerName}]
             ON [{tableName}]
             {trigger.ActionType} {trigger.Action}
             AS
             BEGIN
                 SET NOCOUNT ON;
                 {trigger.Query}
             END";
        using var command = new SqlCommand(triggerSql, connection);
        command.ExecuteNonQuery();
    }

    public void UpdateTrigger(string dbName, string tableName, TriggerModel trigger, string? newTriggerName)
    {
        using var connection = GetConnection(dbName);

        // 1. Check if the trigger exists
        var triggerExistsSql = $@"
        SELECT 1 
        FROM sys.triggers 
        WHERE name = '{trigger.TriggerName?.Replace("'", "''")}' 
        AND parent_id = OBJECT_ID('{tableName.Replace("'", "''")}')";

        bool triggerExists;
        using (var existsCommand = new SqlCommand(triggerExistsSql, connection))
        {
            triggerExists = existsCommand.ExecuteScalar() != null;
        }

        // 2. Only proceed if trigger exists
        if (!triggerExists)
            throw new InvalidOperationException(
                $"Trigger '{trigger.TriggerName}' does not exist on table '{tableName}'");

        // 3. DELETE the existing trigger
        var createSql = $@"
        IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = '{trigger.TriggerName}')
            DROP TRIGGER [{trigger.TriggerName}];";
        using var deleteCommand = new SqlCommand(createSql, connection);
        deleteCommand.ExecuteNonQuery();

        // 4. CREATE the existing trigger Again
        var alterSql = $@"
        CREATE TRIGGER [{newTriggerName}]
        ON [{tableName}]
        {trigger.ActionType} {trigger.Action}
        AS
        BEGIN
            SET NOCOUNT ON;
            {trigger.Query}
        END";

        using var command = new SqlCommand(alterSql, connection);
        command.ExecuteNonQuery();
    }

    public void DeleteTrigger(string dbName, string? triggerName)
    {
        using var connection = GetConnection(dbName);
        var createSql = $@"
        IF EXISTS (SELECT 1 FROM sys.triggers WHERE name = '{triggerName}')
            DROP TRIGGER [{triggerName}];";
        using var command = new SqlCommand(createSql, connection);
        command.ExecuteNonQuery();
    }

    public List<TriggerModel> GetTriggersByTable(string dbName, string tableName)
    {
        using var connection = GetConnection(dbName);
        var triggers = new List<TriggerModel>();
        var createSql = $@"
        SELECT 
            t.name AS TriggerName,
            OBJECT_DEFINITION(t.object_id) AS Definition
        FROM sys.triggers t
        JOIN sys.objects o ON t.parent_id = o.object_id
        WHERE o.name = '{tableName}'";

        using var command = new SqlCommand(createSql, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var definition = reader["Definition"].ToString();
            if (definition == null) throw new Exception();
            triggers.Add(new TriggerModel
            {
                TriggerName = reader["TriggerName"].ToString(),
                Action = ExtractTriggerAction(definition),
                Query = ExtractTriggerQuery(definition),
                ActionType = ExtractTriggerType(definition)
            });
        }

        return triggers;
    }

    public TriggerModel GetTrigger(string dbName, string? triggerName)
    {
        using var connection = GetConnection(dbName);
        var createSql = $@"
        SELECT 
            t.name AS TriggerName,
            OBJECT_DEFINITION(t.object_id) AS Definition,
            o.name AS TableName
        FROM sys.triggers t
        JOIN sys.objects o ON t.parent_id = o.object_id
        WHERE t.name = '{triggerName}'";

        using var command = new SqlCommand(createSql, connection);
        using var reader = command.ExecuteReader();

        if (!reader.Read()) throw new Exception() ;
        var definition = reader["Definition"].ToString();
        if (definition == null) throw new Exception();
        return new TriggerModel
        {
            TriggerName = reader["TriggerName"].ToString(),
            Action = ExtractTriggerAction(definition),
            Query = ExtractTriggerQuery(definition),
            ActionType = ExtractTriggerType(definition)
        };

    }

    private SqlConnection GetConnection(string dbName)
    {
        var connStr = _connectionStringTemplate?.Replace("{DBName}", dbName);
        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }

    #region HelperMethods

    public string ExtractTriggerAction(string definition)
    {
        var match = Regex.Match(definition, @"(?:FOR EACH ROW|AFTER)\s+(INSERT|UPDATE|DELETE)",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.ToUpper() : "INSERT";
    }

    public string ExtractTriggerQuery(string definition)
    {
        var beginMatch = Regex.Match(definition, @"AS\s+BEGIN(.*?)END",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (beginMatch.Success) return beginMatch.Groups[1].Value.Trim();

        // Fallback for different trigger formats
        var asMatch = Regex.Match(definition, @"AS\s*(.*)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return asMatch.Success ? asMatch.Groups[1].Value.Trim() : string.Empty;
    }


    public string ExtractTriggerType(string definition)
    {
        // Pattern to match trigger types (INSTEAD OF, AFTER, FOR)
        var match = Regex.Match(definition,
            @"(INSTEAD\s+OF|AFTER|FOR)\s+(?:INSERT|UPDATE|DELETE|INSERT\s+OR\s+UPDATE|INSERT\s+OR\s+DELETE|UPDATE\s+OR\s+DELETE|INSERT\s+OR\s+UPDATE\s+OR\s+DELETE)",
            RegexOptions.IgnoreCase);

        if (match.Success) return match.Groups[1].Value.ToUpper();

        // Fallback to check if it's a basic trigger without explicit type (defaults to AFTER in SQL Server)
        match = Regex.Match(definition, @"\b(INSERT|UPDATE|DELETE)\b", RegexOptions.IgnoreCase);
        return match.Success ? "AFTER" : "UNKNOWN";
    }

    #endregion
}