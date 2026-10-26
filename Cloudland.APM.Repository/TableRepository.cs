using System.Data;
using ClosedXML.Excel;
using ExcelDataReader;
using Cloudland.APM.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Cloudland.APM.Repository;

public class TableRepository(IConfiguration config) : ITableRepository
{
    private readonly string? _connectionStringTemplate = config.GetConnectionString("TemplateDatabase");

    public void CreateTable(string dbName, TableModel table)
    {
        using var connection = GetConnection(dbName);

        // Create column definitions
        var columnDefinitions = string.Join(", ", table.Columns.Select(c =>
        {
            if (c is { IsComputed: true, ComputedFormula: not null })
                return $"[{c.Name}] AS ({c.ComputedFormula})";
            return $"[{c.Name}] {c.DataType} {(c.IsNullable ? "NULL" : "NOT NULL")}" +
                   (string.IsNullOrEmpty(c.DefaultValue) ? "" : $" DEFAULT ({c.DefaultValue})");
        }));

        // Create the table
        var createSql = $"CREATE TABLE [{table.Name}] ({columnDefinitions})";
        using var createCommand = new SqlCommand(createSql, connection);
        createCommand.ExecuteNonQuery();

        // Add column descriptions if they exist
        foreach (var column in table.Columns.Where(c => !string.IsNullOrWhiteSpace(c.Description)))
        {
            const string descriptionSql = """

                                                      EXEC sp_addextendedproperty 
                                                          @name = N'MS_Description', 
                                                          @value = @description, 
                                                          @level0type = N'SCHEMA', @level0name = N'dbo', 
                                                          @level1type = N'TABLE', @level1name = @tableName, 
                                                          @level2type = N'COLUMN', @level2name = @columnName
                                          """;

            using var descCommand = new SqlCommand(descriptionSql, connection);
            descCommand.Parameters.AddWithValue("@description", column.Description);
            descCommand.Parameters.AddWithValue("@tableName", table.Name);
            descCommand.Parameters.AddWithValue("@columnName", column.Name);
            descCommand.ExecuteNonQuery();
        }

        //Add ForeignKeys to the created table
        foreach (var fk in table.ForeignKeys) AddForeignKey(dbName, fk);

        //Add Index to the created table
        foreach (var index in table.Indexes) AddIndex(dbName, table.Name, index);

        //Add CheckConstraints to the created table
        foreach (var ch in table.CheckConstraints) AddCheckConstraint(dbName, table.Name, ch);
    }

    public void DeleteTable(string dbName, string tableName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"DROP TABLE [{tableName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void AddColumn(string dbName, string tableName, ColumnModel column)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"ALTER TABLE [{tableName}] ADD [{column.Name}] {column.DataType} {(column.IsNullable ? "NULL" : "NOT NULL")}";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();

        //Add Column Description if exists
        if (string.IsNullOrWhiteSpace(column.Description)) return;
        const string descriptionSql = """

                                                  EXEC sp_addextendedproperty 
                                                      @name = N'MS_Description', 
                                                      @value = @description, 
                                                      @level0type = N'SCHEMA', @level0name = N'dbo', 
                                                      @level1type = N'TABLE', @level1name = @tableName, 
                                                      @level2type = N'COLUMN', @level2name = @columnName
                                      """;

        using var descCommand = new SqlCommand(descriptionSql, connection);
        descCommand.Parameters.AddWithValue("@description", column.Description);
        descCommand.Parameters.AddWithValue("@tableName", tableName);
        descCommand.Parameters.AddWithValue("@columnName", column.Name);
        descCommand.ExecuteNonQuery();
    }

    public void UpdateColumn(string dbName, string tableName, string columnName, ColumnModel newColumn)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"ALTER TABLE [{tableName}] ALTER COLUMN [{newColumn.Name}] {newColumn.DataType} {(newColumn.IsNullable ? "NULL" : "NOT NULL")}";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();

        //  Update the column description (if provided)
        if (!string.IsNullOrWhiteSpace(newColumn.Description))
        {
            var descriptionSql = """

                                                 IF EXISTS (
                                                     SELECT 1 FROM sys.extended_properties
                                                     WHERE major_id = OBJECT_ID(@tableName)
                                                     AND minor_id = COLUMNPROPERTY(major_id, @columnName, 'ColumnId')
                                                     AND name = 'MS_Description'
                                                 )
                                                 BEGIN
                                                     -- Update existing description
                                                     EXEC sp_updateextendedproperty 
                                                         @name = N'MS_Description', 
                                                         @value = @description,
                                                         @level0type = N'SCHEMA', @level0name = N'dbo',
                                                         @level1type = N'TABLE', @level1name = @tableName,
                                                         @level2type = N'COLUMN', @level2name = @columnName
                                                 END
                                                 ELSE
                                                 BEGIN
                                                     -- Add new description
                                                     EXEC sp_addextendedproperty 
                                                         @name = N'MS_Description', 
                                                         @value = @description,
                                                         @level0type = N'SCHEMA', @level0name = N'dbo',
                                                         @level1type = N'TABLE', @level1name = @tableName,
                                                         @level2type = N'COLUMN', @level2name = @columnName
                                                 END
                                 """;

            using var descCommand = new SqlCommand(descriptionSql, connection);
            descCommand.Parameters.AddWithValue("@description", newColumn.Description);
            descCommand.Parameters.AddWithValue("@tableName", tableName);
            descCommand.Parameters.AddWithValue("@columnName", newColumn.Name);
            descCommand.ExecuteNonQuery();
        }
    }

    public void DeleteColumn(string dbName, string tableName, string columnName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"ALTER TABLE [{tableName}] DROP COLUMN [{columnName}]";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void AddForeignKey(string dbName, ForeignKeyModel fk)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"ALTER TABLE [{fk.SourceTable}] ADD CONSTRAINT [{fk.Name}] FOREIGN KEY ([{fk.SourceColumn}]) REFERENCES [{fk.ReferenceTable}]([{fk.ReferenceColumn}]) ON DELETE {fk.OnDelete} ON UPDATE {fk.OnUpdate}";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void AddIndex(string dbName, string tableName, IndexModel index)
    {
        using var connection = GetConnection(dbName);
        var unique = index.IsUnique ? "UNIQUE" : "";
        var clustered = index.IsClustered ? "CLUSTERED" : "NONCLUSTERED";
        var columns = string.Join(", ", index.KeyColumns.Select(c => $"[{c}]"));
        var include = index.IncludedColumns.Count > 0
            ? $" INCLUDE ({string.Join(", ", index.IncludedColumns.Select(c => $"[{c}]"))})"
            : "";
        var filter = !string.IsNullOrEmpty(index.FilterCondition) ? $" WHERE {index.FilterCondition}" : "";

        var sql = $"CREATE {unique} {clustered} INDEX [{index.Name}] ON [{tableName}] ({columns}){include}{filter}";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void AddCheckConstraint(string dbName, string tableName, CheckConstraintModel constraint)
    {
        using var connection = GetConnection(dbName);
        var sql = $"ALTER TABLE [{tableName}] ADD CONSTRAINT [{constraint.Name}] CHECK ({constraint.Expression})";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public TableModel GetTableSchema(string dbName, string tableName)
    {
        var tableModel = new TableModel { Name = tableName };

        using var connection = GetConnection(dbName);

        // First, get the table description
        var tableDescQuery = """

                                     SELECT 
                                         ep.value AS TableDescription
                                     FROM 
                                         sys.tables t
                                     LEFT JOIN 
                                         sys.extended_properties ep ON 
                                         ep.major_id = t.object_id AND
                                         ep.minor_id = 0 AND
                                         ep.name = 'MS_Description'
                                     WHERE 
                                         t.name = @tableName
                             """;

        // Get table description
        using (var tableDescCommand = new SqlCommand(tableDescQuery, connection))
        {
            tableDescCommand.Parameters.AddWithValue("@tableName", tableName);
            tableModel.Description = tableDescCommand.ExecuteScalar()?.ToString();
        }

        // query to include columns and columns descriptions
        var columnQuery = """

                                  SELECT 
                                      c.COLUMN_NAME, 
                                      c.DATA_TYPE, 
                                      c.IS_NULLABLE,
                                      ep.value AS Description
                                  FROM 
                                      INFORMATION_SCHEMA.COLUMNS c
                                  LEFT JOIN 
                                      sys.extended_properties ep ON 
                                      ep.major_id = OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME) AND
                                      ep.minor_id = c.ORDINAL_POSITION AND
                                      ep.name = 'MS_Description'
                                  WHERE 
                                      c.TABLE_NAME = @tableName
                                  ORDER BY 
                                      c.ORDINAL_POSITION
                          """;

        using (var command = new SqlCommand(columnQuery, connection))
        {
            command.Parameters.AddWithValue("@tableName", tableName);

            using var reader = command.ExecuteReader();
            while (reader.Read())
                tableModel.Columns.Add(new ColumnModel
                {
                    Name = reader.GetString(0),
                    DataType = reader.GetString(1),
                    IsNullable = reader.GetString(2) == "YES",
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
        }

        // Get foreign keys
        var fkQuery = """

                          SELECT 
                              fk.name AS ForeignKeyName,
                              OBJECT_NAME(fk.parent_object_id) AS SourceTable,
                              COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS SourceColumn,
                              OBJECT_NAME(fk.referenced_object_id) AS ReferenceTable,
                              COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferenceColumn,
                              fk.delete_referential_action_desc AS OnDelete,
                              fk.update_referential_action_desc AS OnUpdate
                          FROM 
                              sys.foreign_keys fk
                          INNER JOIN 
                              sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                          WHERE 
                              OBJECT_NAME(fk.parent_object_id) = @tableName
                          ORDER BY 
                              fk.name
                      """;

        using (var fkCommand = new SqlCommand(fkQuery, connection))
        {
            fkCommand.Parameters.AddWithValue("@tableName", tableName);

            using var reader = fkCommand.ExecuteReader();
            while (reader.Read())
                tableModel.ForeignKeys.Add(new ForeignKeyModel
                {
                    Name = reader.GetString(0),
                    SourceTable = reader.GetString(1),
                    SourceColumn = reader.GetString(2),
                    ReferenceTable = reader.GetString(3),
                    ReferenceColumn = reader.GetString(4),
                    OnDelete = reader.GetString(5),
                    OnUpdate = reader.GetString(6)
                });
        }

        // Get indexes
        var indexQuery = """

                             SELECT 
                                 i.name AS IndexName,
                                 i.is_unique AS IsUnique,
                                 i.is_primary_key AS IsPrimaryKey,
                                 i.type_desc AS IndexType,
                                 STUFF((
                                     SELECT ', ' + COL_NAME(ic.object_id, ic.column_id)
                                     FROM sys.index_columns ic
                                     WHERE ic.object_id = i.object_id 
                                       AND ic.index_id = i.index_id
                                       AND ic.is_included_column = 0
                                     ORDER BY ic.key_ordinal
                                     FOR XML PATH('')
                                 ), 1, 2, '') AS KeyColumns,
                                 STUFF((
                                     SELECT ', ' + COL_NAME(ic.object_id, ic.column_id)
                                     FROM sys.index_columns ic
                                     WHERE ic.object_id = i.object_id 
                                       AND ic.index_id = i.index_id
                                       AND ic.is_included_column = 1
                                     ORDER BY ic.key_ordinal
                                     FOR XML PATH('')
                                 ), 1, 2, '') AS IncludedColumns,
                                 i.filter_definition AS FilterCondition
                             FROM 
                                 sys.indexes i
                             WHERE 
                                 i.object_id = OBJECT_ID(@tableName)
                                 AND i.name IS NOT NULL
                                 AND i.is_primary_key = 0
                             ORDER BY 
                                 i.name
                         """;

        using (var indexCommand = new SqlCommand(indexQuery, connection))
        {
            indexCommand.Parameters.AddWithValue("@tableName", tableName);

            using var reader = indexCommand.ExecuteReader();
            while (reader.Read())
                tableModel.Indexes.Add(new IndexModel
                {
                    Name = reader.GetString(0),
                    IsUnique = reader.GetBoolean(1),
                    IsClustered = reader.GetString(3) == "CLUSTERED",
                    KeyColumns = reader.IsDBNull(4)
                        ? new List<string>()
                        : reader.GetString(4).Split(',').Select(x => x.Trim()).ToList(),
                    IncludedColumns = reader.IsDBNull(5)
                        ? new List<string>()
                        : reader.GetString(5).Split(',').Select(x => x.Trim()).ToList(),
                    FilterCondition = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
        }

        // Get check constraints
        var checkQuery = """

                             SELECT 
                                 cc.name AS ConstraintName,
                                 cc.definition AS Expression,
                                 ep.value AS Description
                             FROM 
                                 sys.check_constraints cc
                             LEFT JOIN 
                                 sys.extended_properties ep ON 
                                 ep.major_id = cc.object_id AND
                                 ep.minor_id = 0 AND
                                 ep.name = 'MS_Description'
                             WHERE 
                                 OBJECT_NAME(cc.parent_object_id) = @tableName
                         """;

        using (var checkCommand = new SqlCommand(checkQuery, connection))
        {
            checkCommand.Parameters.AddWithValue("@tableName", tableName);

            using var reader = checkCommand.ExecuteReader();
            while (reader.Read())
                tableModel.CheckConstraints.Add(new CheckConstraintModel
                {
                    Name = reader.GetString(0),
                    Expression = reader.GetString(1),
                    Description = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
        }

        // Get primary key columns
        var pkQuery = """

                          SELECT 
                              COL_NAME(ic.object_id, ic.column_id) AS ColumnName
                          FROM 
                              sys.indexes i
                          INNER JOIN 
                              sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                          WHERE 
                              i.object_id = OBJECT_ID(@tableName)
                              AND i.is_primary_key = 1
                          ORDER BY 
                              ic.key_ordinal
                      """;

        using (var pkCommand = new SqlCommand(pkQuery, connection))
        {
            pkCommand.Parameters.AddWithValue("@tableName", tableName);

            using var reader = pkCommand.ExecuteReader();
            while (reader.Read()) tableModel.PrimaryKeyColumns.Add(reader.GetString(0));
        }

        return tableModel;
    }

    public bool TableExists(string dbName, string tableName)
    {
        using var connection = GetConnection(dbName);
        var sql = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
        using var command = new SqlCommand(sql, connection);
        var count = (int)command.ExecuteScalar();
        return count > 0;
    }

    public void SetTableDescription(string dbName, string tableName, string description)
    {
        using var connection = GetConnection(dbName);
        var sql =
            $"EXEC sp_addextendedproperty @name=N'MS_Description', @value=N'{description}', @level0type=N'Schema', @level0name=N'dbo', @level1type=N'Table', @level1name=N'{tableName}'";
        using var command = new SqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void ImportFromExcel(string dbName, string tableName, Stream excelFileStream)
    {
        using var reader = ExcelReaderFactory.CreateReader(excelFileStream);
        var dataSet = reader.AsDataSet();
        var dataTable = dataSet.Tables[0]; // Assuming the first sheet is the target

        using var connection = GetConnection(dbName);
        using var transaction = connection.BeginTransaction();

        try
        {
            // Create a bulk copy instance for efficient data import
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.DestinationTableName = tableName;

            // Map columns
            foreach (DataColumn column in dataTable.Columns)
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

            // Perform the bulk copy
            bulkCopy.WriteToServer(dataTable);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw new Exception($"Failed to import Excel data to table '{tableName}': {ex.Message}", ex);
        }
    }


    public Stream ExportToExcel(string dbName, string tableName)
    {
        var memoryStream = new MemoryStream();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(tableName);

        using var connection = GetConnection(dbName);
        var sql = $"SELECT * FROM [{tableName}]";
        using var command = new SqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        // Add headers
        for (var i = 0; i < reader.FieldCount; i++) worksheet.Cell(1, i + 1).Value = reader.GetName(i);

        // Add rows
        var rowIndex = 2;
        while (reader.Read())
        {
            for (var colIndex = 0; colIndex < reader.FieldCount; colIndex++)
            {
                var value = reader.GetValue(colIndex);
                worksheet.Cell(rowIndex, colIndex + 1).Value = Convert.ToString(value);
            }

            rowIndex++;
        }

        // Format the worksheet
        worksheet.Columns().AdjustToContents();

        // Save to the memory stream
        workbook.SaveAs(memoryStream);
        memoryStream.Position = 0; // Reset the stream position for reading
        return memoryStream;
    }

    private SqlConnection GetConnection(string dbName)
    {
        var connStr = _connectionStringTemplate?.Replace("{DBName}", dbName);
        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }
}