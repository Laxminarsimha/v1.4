using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PhixDataFlow.Startup))]
namespace PhixDataFlow
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
