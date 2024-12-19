using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace SiteMonitor.Web.Attributes;

public class AuditLogAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var logger = context.HttpContext.RequestServices.GetService<ILogger<AuditLogAttribute>>();
        var user = context.HttpContext.User.Identity?.Name ?? "Anonymous";
        var action = context.ActionDescriptor.DisplayName;

        logger.LogInformation(
            "User {User} executing action {Action} at {Time}",
            user,
            action,
            DateTime.UtcNow);

        base.OnActionExecuting(context);
    }
}