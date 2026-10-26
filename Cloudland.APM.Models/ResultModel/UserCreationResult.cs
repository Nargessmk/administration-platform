using io.fusionauth.domain;

namespace Cloudland.APM.Models.ResultModel;

public class UserCreationResult
{
    public User? User { get; set; }
    public List<Dictionary<string, string>>? Errors { get; set; }
}