using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KILVRA.Area.Admin
{
 // Simple action filter that enforces admin session
 public class AdminAuthorizeAttribute : ActionFilterAttribute
 {
 public override void OnActionExecuting(ActionExecutingContext context)
 {
 var httpContext = context.HttpContext;
 var adminId = httpContext.Session.GetInt32("AdminId");
 if (adminId == null)
 {
 // redirect to AdminAccount Login
 context.Result = new RedirectToActionResult("Login", "AdminAccount", new { area = "Admin" });
 return;
 }

 base.OnActionExecuting(context);
 }
 }
}
