using System.Security.Claims;
using System.Web;
using AzureBootCampTickets.Contracts.Services;

namespace AzureBootCampTickets.Web.Services
{
    public class IdentiService : IIdentiService
    {

        public string GetUserId()
        {
            var userId =  ((ClaimsPrincipal)HttpContext.Current.User).FindFirst(ClaimTypes.NameIdentifier).Value;

            return userId;
        }
        public string GetUserEmail()
        {
            var email = ((ClaimsPrincipal)HttpContext.Current.User).FindFirst(ClaimTypes.Name).Value;
            return email;
        }
    }
}