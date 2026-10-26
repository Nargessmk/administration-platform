using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface IStoredProcedureRepository
{
    void CreateStoredProcedure(string dbName, StoredProcedureModel sp);
    void DeleteStoredProcedure(string dbName, string spName);
    StoredProcedureModel GetStoredProcedure(string dbName, string spName);
    List<StoredProcedureModel> GetAllStoredProcedures(string dbName);
    void SetStoredProcedureDescription(string dbName, string spName, string description);
}