namespace Cloudland.APM.Models.RequestModel;

public class TriggerRequest
{
    public required TriggerModel TriggerModel { get; set; }
    public required string TableName { get; set; }
    public string? NewTriggerName { get; set; } = string.Empty;
}