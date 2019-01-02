using System.Web.Http;

namespace V4NetFrameworkBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(BotConfig.Register);
        }
    }
}
