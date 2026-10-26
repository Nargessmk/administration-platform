namespace Cloudland.APM.Models;

public class StoredProcedureModel
{
    public string Name { get; set; } = string.Empty;
    public string DefinitionSql { get; set; } = string.Empty; // متن CREATE PROCEDURE
    public string? Description { get; set; }
}