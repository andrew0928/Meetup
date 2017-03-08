using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DemoWeb.Startup))]
namespace DemoWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
