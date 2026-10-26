namespace Cloudland.APM.Models;

public class TriggerModel
{
    public string? TriggerName { get; set; } = string.Empty;

    //INSERT,DELETE,UPDATE
    public string Action { get; set; } = string.Empty;

    //AFTER,FOR/INSTEAD OF
    public string ActionType { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
}