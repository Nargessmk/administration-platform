namespace Cloudland.APM.DTO;

public class FolderItemDTO
{
    public string? ParentTitle { get; set; }
    public Guid ParentNodeTypeId { get; set; }
    public string? FolderTitle { get; set; }
    public string? FolderIcon { get; set; }
}