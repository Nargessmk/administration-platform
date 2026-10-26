namespace Cloudland.APM.Models.RequestModel;

public class PersianQueryDbRequest
{
    public required string DbName { get; set; } // نام دیتابیس
    public required string Query { get; set; }
} //}