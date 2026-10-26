using Cloudland.APM.DTO;

namespace Cloudland.SSSB.DTO;

public class VehicleDTO : BaseDTO<Guid>
{
    public Guid VehicleTypeId { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public string VehicleTypeTitle { get; set; } = string.Empty;
    public string? TypeTitle { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string? ValidityDate { get; set; } = string.Empty;
}
