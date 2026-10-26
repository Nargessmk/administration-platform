namespace Cloudland.APM.DTO;

public sealed class TwoFactorDisableRequestDTO
{
    public Guid UserId { get; set; }
    public string? Code { get; set; } = string.Empty;
}
