using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Cloudland.SSSB.Microservices.AppManagement.Extensions;

public sealed class ModelValidationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
            context.Result = new BadRequestObjectResult(new
                { model = context.ModelState, message = "خطا در عملیات اطلاعات" });
    }
}