using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MimoHitoma2.Startup))]
namespace MimoHitoma2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
