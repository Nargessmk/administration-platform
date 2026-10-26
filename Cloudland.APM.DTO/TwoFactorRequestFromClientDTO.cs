namespace Cloudland.APM.DTO;

public sealed class TwoFactorRequestFromClientDTO
{
    public Guid UserId { get; set; }
    public string? secret { get; set; }
    public string? secretBase32Encoded { get; set; }
    public string? code { get; set; }
}