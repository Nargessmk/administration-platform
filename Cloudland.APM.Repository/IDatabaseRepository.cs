using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface IDatabaseRepository
{
    void CreateDatabase(string? dbName);
    void DeleteDatabase(string? dbName);
    void RenameDatabase(string? oldName, string? newName);
    List<DatabaseModel> GetAllDatabases();
    DatabaseModel GetDatabaseSchema(string? dbName);
    bool DatabaseExists(string? dbName);
    void SetDatabaseDescription(string? dbName, string description);
    string GetDatabaseDescription(string? dbName);
}