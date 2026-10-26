using System.Text.Json;
using Cloudland.APM.DTO.NodeObjects;

namespace Cloudland.APM.Microservices.AppManagement.Services;

public class DtoTypeService : IDtoTypeService
{
    // Public Properties
    public IReadOnlyDictionary<string, Type> DtoTypeMap { get; } = CreateDtoTypeMap();
    public JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null
    };

    // This dictionary maps user-friendly node types to their corresponding DTO class names.
    private readonly IReadOnlyDictionary<string, string> _mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["application"] = "ApplicationDTO",
        ["database"] = "DatabaseDTO",
        ["table"] = "TableDTO",
        ["general"] = "PublicSettingDTO",
        ["search field"] = "SearchFieldDTO",
        ["display field"] = "DisplayFieldDTO",
        ["field"] = "FieldDTO",
        ["dashboard subset"] = "SubDashboardDTO",
        ["attachment"] = "TableAttachmentDTO",
        ["dashboard integration"] = "DashboardIntegrationDTO",
        ["show field event"] = "ShowFieldEventDTO",
        ["event"] = "TableEventDTO",
        ["application manager setting"] = "AdminSettingDTO",
        ["grid row color"] = "GridRowColorDTO",
        ["function"] = "TableFunctionDTO",
        ["calculation field"] = "ComputationalFieldDTO",
        ["button"] = "ButtonDTO",
        ["folder"] = "FolderDTO",
        ["form"] = "InformationEntryFormDTO",
        ["dashboard"] = "DashboardDTO",
        ["special phrase"] = "SpecialPhraseDTO",
        ["new button form"] = "NewButtonFormDTO",
        ["report"] = "ReportDTO",
        ["web service"] = "WebServiceDTO",
    };

    public string ConvertToDtoTypeName(string nodeType)
    {
        if (_mappings.TryGetValue(nodeType, out var dtoTypeName))
        {
            return dtoTypeName;
        }
        throw new NotSupportedException($"No mapping found for node type: {nodeType}");
    }

    /// <summary>
    /// Scans the DTO assembly for all concrete DTO types in the specified namespace
    /// and creates a map from the type name to the Type object.
    /// </summary>
    /// <returns>A read-only dictionary mapping DTO names to their types.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if duplicate DTO class names are found. This prevents silently ignoring
    /// a potential configuration error.
    /// </exception>
    private static IReadOnlyDictionary<string, Type> CreateDtoTypeMap()
    {
        const string dtoNamespace = "Cloudland.APM.DTO.NodeObjects";
        var assembly = typeof(TableDTO).Assembly;

        // Materialize the LINQ query into a list to prevent multiple enumerations.
        var dtoTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, Namespace: dtoNamespace })
            .ToList();

        try
        {
            // ToDictionary now operates on the in-memory list.
            // It will throw an ArgumentException on the first duplicate found.
            return dtoTypes.ToDictionary(t => t.Name, t => t);
        }
        catch (ArgumentException ex)
        {
            // If duplicates exist, find all of them to provide a helpful error message.
            // This also operates on the same in-memory list.
            var duplicateTypeNames = dtoTypes
                .GroupBy(t => t.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            throw new InvalidOperationException(
                $"Duplicate DTO type names found in namespace '{dtoNamespace}': {string.Join(", ", duplicateTypeNames)}. Please resolve the naming conflict.",
                ex);
        }
    }
}