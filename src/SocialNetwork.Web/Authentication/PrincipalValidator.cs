using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using SocialNetwork.Core.Repositories;
using SocialNetwork.Core.Utils;
using SocialNetwork.Web.Utils;

namespace SocialNetwork.Web.Authentication
{
    public static class PrincipalValidator
    {
        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Principal == null) throw new ArgumentNullException(nameof(context.Principal));

            var username = context.Principal.TryGetUsername();
            if (username == null)
            {
                context.RejectPrincipal();
                return;
            }
            
            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            var user = await userRepository.GetUserAsync(username);
            if (user == null)
            {
                context.RejectPrincipal();
                return;
            }

            var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContext>();
            userContext.CurrentUser = user;
        }
    }
}