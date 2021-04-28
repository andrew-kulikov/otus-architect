﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocialNetwork.Core.Utils;

namespace SocialNetwork.Web.Controllers
{
    public class UserActionControllerBase : Controller
    {
        protected readonly IUserContext UserContext;

        public UserActionControllerBase(IUserContext userContext)
        {
            UserContext = userContext;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (UserContext.CurrentUser.Profile == null)
            {
                context.Result = new RedirectToActionResult("CreateProfile", "Registration", context.RouteData);
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}