using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloudland.APM.Microservices.ReportGenerator.ActionFilters;

public sealed class LogRequestResponseFilter(ILogger<LogRequestResponseFilter> logger) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
        var controllerName = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;
        var actionName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName;

        logger.LogInformation(
            "درخواست ورودی: IP={IpAddress} به {ControllerName}.{ActionName} در زمان {Timestamp}",
            ipAddress,
            controllerName,
            actionName,
            DateTime.UtcNow
        );

        await next();

        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds;

        logger.LogInformation(
            "پاسخ خروجی: IP={IpAddress} از {ControllerName}.{ActionName} | مدت زمان: {Duration}ms در زمان {Timestamp}",
            ipAddress,
            controllerName,
            actionName,
            duration,
            DateTime.UtcNow
        );
    }
}