using System;
using System.Linq;

using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(Example.Azure.Startup))]

namespace Example.Azure
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}