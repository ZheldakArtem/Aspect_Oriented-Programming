using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace AOP.Samples
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			Util.AutofacConfig.ConfigureContainer();

			AreaRegistration.RegisterAllAreas();
			RouteConfig.RegisterRoutes(RouteTable.Routes);
		}
	}
}
