namespace Cloudland.APM.DTO;

public sealed class TwoFactorSecretDetailDTO
{
    public string? secret { get; set; }
    public string? secretBase32Encoded { get; set; }
}