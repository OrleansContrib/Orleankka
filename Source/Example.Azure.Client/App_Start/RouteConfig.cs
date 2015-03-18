using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Example.Azure
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Spawn", id = UrlParameter.Optional }
            );
        }
    }
}
