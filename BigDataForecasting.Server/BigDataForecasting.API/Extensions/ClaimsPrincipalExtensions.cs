using System.Security.Claims;

namespace BigDataForecasting.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim==null)
            
                throw new Exception("Token içinde kullanıcı ID'si bulunamadı! ");
            return int.Parse(claim.Value);
            
        }
    }
}
