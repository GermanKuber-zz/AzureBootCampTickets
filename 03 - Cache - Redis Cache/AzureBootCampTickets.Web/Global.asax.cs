

using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using AzureBootCampTickets.Cache;
using AzureBootCampTickets.Contracts;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.ApplicationDb;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Data.Context.CloudContext;
using AzureBootCampTickets.Data.Repositories;
using AzureBootCampTickets.Services;
using AzureBootCampTickets.Web.Services;

namespace AzureBootCampTickets.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var builder = new ContainerBuilder();


            builder.RegisterType<IdentiService>().As<IIdentiService>();
            builder.RegisterType<EventsRepository>().As<IEventsRepository>();
            builder.RegisterType<TicketsRepository>().As<ITicketsRepository>();
            builder.RegisterType<OrderService>().As<IOrderService>();
            builder.RegisterType<EventManagementService>().As<IEventManagementService>();
            //TODO : 03 - Registro la dependencia
            builder.RegisterType<CacheService>().As<ICacheService>();

            builder.RegisterType<AzureBootCampTicketsCloudContext>().As<IAzureBootCampTicketsCloudContext>();
            builder.RegisterType<ApplicationDbContext>().As<ApplicationDbContext>();
            builder.RegisterType<AzureBootCampTicketsContext>().As<AzureBootCampTicketsContext>();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            builder.RegisterModule<AutofacWebTypesModule>();
            builder.RegisterSource(new ViewRegistrationSource());
            builder.RegisterFilterProvider();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));


        }
    }
}
