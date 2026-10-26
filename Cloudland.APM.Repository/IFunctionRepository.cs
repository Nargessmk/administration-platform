using Cloudland.APM.Models;

namespace Cloudland.APM.Repository;

public interface IFunctionRepository
{
    void CreateFunction(string dbName, FunctionModel function);
    void DeleteFunction(string dbName, string functionName);
    FunctionModel GetFunction(string dbName, string functionName);
    List<FunctionModel> GetAllFunctions(string dbName);
    void SetFunctionDescription(string dbName, string functionName, string description);
}