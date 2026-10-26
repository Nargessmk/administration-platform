namespace Cloudland.APM.DTO;

public static class FolderConstants
{
    public static readonly List<FolderItemDTO> FolderList =
    [
        new()
        {
            ParentTitle = "جدول",
            ParentNodeTypeId = Guid.Parse("47dbacd7-4f0d-4022-a044-7c7d1beff42f"),
            FolderTitle = "فیلد",
            FolderIcon = "table-rows",
        },
        new()
        {
            ParentTitle = "جدول",
            ParentNodeTypeId = Guid.Parse("47dbacd7-4f0d-4022-a044-7c7d1beff42f"),
            FolderTitle = "رویداد",
            FolderIcon = "code"
        },
        new()
        {
            ParentTitle = "جدول",
            ParentNodeTypeId = Guid.Parse("47dbacd7-4f0d-4022-a044-7c7d1beff42f"),
            FolderTitle = "ضمیمه",
            FolderIcon = "file-invoice"
        }
        // Add more items as needed
    ];
}