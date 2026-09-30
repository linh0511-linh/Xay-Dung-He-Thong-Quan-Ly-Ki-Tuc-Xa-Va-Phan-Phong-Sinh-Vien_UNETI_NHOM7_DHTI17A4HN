using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyKyTucXa_UNETIxx_xxx.Filters;

public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    private readonly string? _role;
    public SessionAuthorizeAttribute(string? role = null) => _role = role;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        var role = context.HttpContext.Session.GetString("Role");

        if (userId == null)
            context.Result = new RedirectToActionResult("Login", "Account", null);
        else if (_role != null && role != _role)
            context.Result = new RedirectToActionResult("Login", "Account", null);
    }
}