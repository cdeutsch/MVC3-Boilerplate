using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Web.Models;
using System.Data.Entity.Database;

namespace Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
            routes.MapRoute(
                "Login", // Route name
                "login", // URL with parameters
                new { controller = "Session", action = "login" } // Parameter defaults
            );
            routes.MapRoute(
                "Logout", // Route name
                "logout", // URL with parameters
                new { controller = "Session", action = "logout" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            //initialize EF Code First DB.
            DbDatabase.SetInitializer<SiteDB>(new SiteDBInitializer(new AccountMembershipService()));

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error()
        {
            Exception lastException = Server.GetLastError();
            // Log the exception.
            Elmah.ErrorSignal.FromCurrentContext().Raise(lastException);
        }
    }
}