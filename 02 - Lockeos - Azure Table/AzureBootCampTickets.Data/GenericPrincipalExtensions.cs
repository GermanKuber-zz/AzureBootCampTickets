using System.Security.Claims;
using System.Security.Principal;
using AzureBootCampTickets.Data.Context.ApplicationDb;
using AzureBootCampTickets.Entities.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AzureBootCampTickets.Data
{
    public static class GenericPrincipalExtensions
    {
        public static ApplicationUser ApplicationUser(this IPrincipal user)
        {
            ClaimsPrincipal userPrincipal = (ClaimsPrincipal)user;
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            if (userPrincipal.Identity.IsAuthenticated)
            {
                return userManager.FindById(userPrincipal.Identity.GetUserId());
            }
            else
            {
                return null;
            }
        }
    }
}