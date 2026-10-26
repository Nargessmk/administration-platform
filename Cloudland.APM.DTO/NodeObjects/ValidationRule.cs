namespace Cloudland.APM.DTO.NodeObjects;

public class ValidationRule
{
    public required string Title { get; set; }
    public required string Name { get; set; }
    public required string InputType { get; set; }
    public required string Placeholder { get; set; }
    public string? Icon { get; set; }
    public bool IsReadonly { get; set; }
    public bool IsRequired { get; set; }
    public bool Disabled { get; set; }
    public List<DropdownOption>? Options { get; set; } 
}