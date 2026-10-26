using System.Text.Json;
namespace Cloudland.APM.Microservices.AppManagement.Services;

public interface IDtoTypeService
{
    IReadOnlyDictionary<string, Type> DtoTypeMap { get; }
    JsonSerializerOptions JsonOptions { get; }
    string ConvertToDtoTypeName(string nodeType);
}