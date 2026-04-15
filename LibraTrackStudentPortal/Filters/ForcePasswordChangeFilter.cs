using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ForcePasswordChangeFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var mustChange = context.HttpContext.Session.GetString("MustChangePassword");

        if (mustChange == "true")
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            // ONLY allow access to password page
            if (!(controller == "Student" && action == "ForceChangePassword"))
            {
                context.Result = new RedirectToActionResult(
                    "ForceChangePassword",
                    "Student",
                    null
                );
            }
        }

        base.OnActionExecuting(context);
    }
}