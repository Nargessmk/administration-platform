using io.fusionauth.domain;

namespace Cloudland.APM.Models.ResultModel;

public class RoleResult
{
    public string ApplicationName { get; set; } = string.Empty;
    public ApplicationRole? Role { get; set; }
    public List<User>? Users { get; set; }
}