using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Orleankka.Client;

namespace Example.Azure
{
    public class MvcApplication : HttpApplication
    {
        public static ClientActorSystem System;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
