using Microsoft.EntityFrameworkCore;

namespace APPSEC_Assignment2.Model
{
    public class TokenExpirationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenExpirationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                // Check if the token has expired 

                var authTokenExpiration = context.Session.GetString("AuthToken");
                if (!string.IsNullOrEmpty(authTokenExpiration) && DateTime.TryParse(authTokenExpiration, out var expirationTime))
                {
                    if (expirationTime < DateTime.Now)
                    {
                        // Token has expired, redirect the user to the login page
                        context.Response.Redirect("/Login");
                        return;
                    }
                }
            }

            // Continue processing the request
            await _next(context);
        }


    }
}
