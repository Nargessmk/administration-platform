namespace Cloudland.APM.Models;

public enum FunctionType
{
    Scalar,
    InlineTableValued,
    MultiStatementTableValued
}

public class FunctionModel
{
    public string Name { get; set; } = string.Empty;
    public FunctionType Type { get; set; }
    public string DefinitionSql { get; set; } = string.Empty; // متن CREATE FUNCTION
    public string? Description { get; set; }
}