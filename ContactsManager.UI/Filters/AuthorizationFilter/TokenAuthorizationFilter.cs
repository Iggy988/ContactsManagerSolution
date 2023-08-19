using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.AuthorizationFilter;

public class TokenAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        //request cookies is created to browser and submited to server -> we are trying to read all coockies that are send from browser to server
        // pokusavamo da citamo cookie koji se odnosi na authorization
        if (context.HttpContext.Request.Cookies.ContainsKey("Auth-Key") == false)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
            return;
        }

        if (context.HttpContext.Request.Cookies["Auth-Key"] != "A100")
        {
            context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
        }
    }
}
