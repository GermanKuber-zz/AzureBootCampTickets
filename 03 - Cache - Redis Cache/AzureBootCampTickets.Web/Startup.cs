using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AzureBootCampTickets.Web.Startup))]
namespace AzureBootCampTickets.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
