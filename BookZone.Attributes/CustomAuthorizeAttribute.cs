using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace BookZone.Attributes
{
    public class CustomAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Auth" },
                    { "action", "Index" },
                    { "area", "Auth" }
                });
            }
            else
            {
                // Set the user identity
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                };
                var identity = new ClaimsIdentity(claims, "Custom");
                context.HttpContext.User = new ClaimsPrincipal(identity);
            }
            base.OnActionExecuting(context);
        }
    }
}
