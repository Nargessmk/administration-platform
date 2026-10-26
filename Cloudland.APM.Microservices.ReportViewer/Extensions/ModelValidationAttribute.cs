using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloudland.APM.Microservices.ReportViewer.Extensions;

public sealed class ModelValidationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
            context.Result = new BadRequestObjectResult(new
                { model = context.ModelState, message = "خطا در عملیات اطلاعات" });
    }
}