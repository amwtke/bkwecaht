using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BK_WeChat.Startup))]
namespace BK_WeChat
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
