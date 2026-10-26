namespace Cloudland.APM.Models;

public class ForeignKeyModel
{
    public string Name { get; set; } = string.Empty;
    public string SourceTable { get; set; } = string.Empty; // این می‌تواند ضمنی همان TableModel باشد
    public string SourceColumn { get; set; } = string.Empty;
    public string ReferenceTable { get; set; } = string.Empty;
    public string ReferenceColumn { get; set; } = string.Empty;
    public string OnDelete { get; set; } = "NO ACTION"; // یا CASCADE/SET NULL
    public string OnUpdate { get; set; } = "NO ACTION";
}