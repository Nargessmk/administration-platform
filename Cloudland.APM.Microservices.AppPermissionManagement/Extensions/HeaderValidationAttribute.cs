using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloudland.APM.Microservices.AppPermissionManagement.Extensions;

public sealed class HeaderValidationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers["companyid"].Any())
            context.Result = new BadRequestObjectResult(new
                { model = context.ModelState, message = "شناسه اختصاصی فرد/شرکت صحیح نمی باشد" });
    }
}