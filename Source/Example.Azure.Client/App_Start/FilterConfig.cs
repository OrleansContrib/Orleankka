using System;
using System.Linq;
using System.Web.Mvc;

namespace Example.Azure
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
