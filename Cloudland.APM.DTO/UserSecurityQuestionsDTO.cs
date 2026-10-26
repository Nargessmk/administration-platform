namespace Cloudland.APM.DTO;

public class UserSecurityQuestionsDTO
{
    public required string Email { get; set; }
    public required List<SecurityQuestionsDTO> Questions { get; set; }
}

