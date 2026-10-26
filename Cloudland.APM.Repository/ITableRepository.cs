using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface ITableRepository
{
    void CreateTable(string dbName, TableModel table);
    void DeleteTable(string dbName, string tableName);
    void AddColumn(string dbName, string tableName, ColumnModel column);
    void UpdateColumn(string dbName, string tableName, string columnName, ColumnModel newDefinition);
    void DeleteColumn(string dbName, string tableName, string columnName);
    void AddForeignKey(string dbName, ForeignKeyModel fk);
    void AddIndex(string dbName, string tableName, IndexModel index);
    void AddCheckConstraint(string dbName, string tableName, CheckConstraintModel model);
    TableModel GetTableSchema(string dbName, string tableName);
    bool TableExists(string dbName, string tableName);
    void SetTableDescription(string dbName, string tableName, string description);
    void ImportFromExcel(string dbName, string tableName, Stream excelFileStream);
    Stream ExportToExcel(string dbName, string tableName);
}