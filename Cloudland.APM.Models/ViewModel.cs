namespace Cloudland.APM.Models;

public class ViewModel
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionSql { get; set; } = string.Empty; // کوئری SELECT تعریف View
    public string? Description { get; set; }
}