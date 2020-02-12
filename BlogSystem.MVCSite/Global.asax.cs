using BlogSystem.MVCSite.Controllers;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BlogSystem.MVCSite
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            if (exception is HttpException && ((HttpException)exception).GetHttpCode() == 404)
            {
                ErrorController.exception = exception;
                Server.ClearError();
                Response.Redirect("/Error/NotFoundError",true);
            }
            else if (exception is HttpException && ((HttpException)exception).GetHttpCode() == 500)
            {
                ErrorController.exception = exception;
                Server.ClearError();
                Response.Redirect("/Error/InternalError",true);
            }
            else
            {
                var ip = HttpContext.Current.Request.UserHostAddress;
                Logger.Warn(exception, ip +" : ÆäËû´íÎó");
            }
        }
    }
}
