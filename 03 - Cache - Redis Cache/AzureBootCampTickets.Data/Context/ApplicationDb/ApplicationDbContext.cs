using AzureBootCampTickets.Entities.Entities;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AzureBootCampTickets.Data.Context.ApplicationDb
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
       
    }
}
