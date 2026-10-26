using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface ITriggerRepository
{
    void CreateTrigger(string dbName, string tableName, TriggerModel trigger);
    void UpdateTrigger(string dbName, string tableName, TriggerModel trigger, string? newTriggerName);
    void DeleteTrigger(string dbName, string? triggerName);
    List<TriggerModel> GetTriggersByTable(string dbName, string tableName);
    TriggerModel GetTrigger(string dbName, string? triggerName);
    string ExtractTriggerAction(string definition);
    string ExtractTriggerQuery(string definition);
    public string ExtractTriggerType(string definition);
}