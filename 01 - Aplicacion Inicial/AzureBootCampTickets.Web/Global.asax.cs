using System;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.ApplicationDb;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Data.Repositories;
using AzureBootCampTickets.Services;
using AzureBootCampTickets.Web.Services;
using Microsoft.Owin.Logging;
using WebGrease;

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

            // Register individual components

            builder.RegisterType<IdentiService>().As<IIdentiService>();
            builder.RegisterType<EventsRepository>().As<IEventsRepository>();
            builder.RegisterType<TicketsRepository>().As<ITicketsRepository>();
            builder.RegisterType<OrderService>().As<IOrderService>();
            builder.RegisterType<EventManagementService>().As<IEventManagementService>();

            builder.RegisterType<ApplicationDbContext>().As<ApplicationDbContext>();
            builder.RegisterType<AzureBootCampTicketsContext>().As<AzureBootCampTicketsContext>();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(typeof(MvcApplication).Assembly);
            builder.RegisterModelBinderProvider();

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            // OPTIONAL: Enable action method parameter injection (RARE).
       
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));


        }
    }
}
