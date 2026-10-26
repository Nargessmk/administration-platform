namespace Cloudland.APM.Models;

public class DatabaseModel
{
    public string? Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TableModel> Tables { get; set; } = new();
    public List<ViewModel> Views { get; set; } = new();
    public List<StoredProcedureModel> StoredProcedures { get; set; } = new();
    public List<FunctionModel> Functions { get; set; } = new();
}