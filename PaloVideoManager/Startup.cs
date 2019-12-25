using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PaloVideoManager.Startup))]
namespace PaloVideoManager
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
