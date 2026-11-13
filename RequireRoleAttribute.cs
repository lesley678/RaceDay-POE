using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.Filters;
namespace RaceDay.MVC.Security;
public sealed class RequireRoleAttribute(string? role=null):ActionFilterAttribute { public override void OnActionExecuting(ActionExecutingContext c){var s=c.HttpContext.Session;if(!s.GetInt32("UserId").HasValue){c.Result=new RedirectToActionResult("Login","Account",new{ returnUrl=c.HttpContext.Request.Path});return;}if(role!=null&&!string.Equals(s.GetString("UserRole"),role,StringComparison.OrdinalIgnoreCase)){c.Result=new RedirectToActionResult("AccessDenied","Home",null);}} }
