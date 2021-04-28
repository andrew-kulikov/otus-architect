using System;
using System.Linq;
using System.Security.Claims;

namespace SocialNetwork.Web.Utils
{
    public static class ContextExtension
    {
        public static long GetUserId(this ClaimsPrincipal principal)
        {
            var idClaim = principal.Claims.FirstOrDefault(claim => claim.Type == Constants.ClaimTypes.UserId)?.Value;

            if (!long.TryParse(idClaim, out var id)) throw new ArgumentException("Invalid id claim");

            return id;
        }

        public static string GetUsername(this ClaimsPrincipal principal)
        {
            var usernameClaim = TryGetUsername(principal);

            if (string.IsNullOrEmpty(usernameClaim)) throw new ArgumentException("Invalid username claim");

            return usernameClaim;
        }

        public static string TryGetUsername(this ClaimsPrincipal principal)
        {
            return principal.Claims.FirstOrDefault(claim => claim.Type == Constants.ClaimTypes.Username)?.Value;
        }
    }

    public static class Constants
    {
        public static class ClaimTypes
        {
            public const string Username = "SocialNetwork.Username";
            public const string UserId = "SocialNetwork.UserId";
        }
    }
}