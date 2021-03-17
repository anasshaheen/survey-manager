using System.Linq;
using System.Security.Claims;

namespace SurveyManager.API.Helpers
{
    public static class ClaimsHelpers
    {
        public static int UserId(this ClaimsPrincipal claims)
        {
            return int.Parse(claims.Claims.First(e => e.Type == "UserId").Value);
        }

        public static int? OptionalUserId(this ClaimsPrincipal claims)
        {
            var userIdClaim = claims.Claims.FirstOrDefault(e => e.Type == "UserId");
            if (userIdClaim == null)
            {
                return null;
            }

            return int.Parse(userIdClaim.Value);
        }
    }
}