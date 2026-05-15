using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TESTFRAMEWORK
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();

            // Log to file
            LogError(ex);

            // Clear the error
            Server.ClearError();

            // Redirect to Error page
            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Response.Redirect("/Home/Error");
            }
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/App_Data/Logs");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }

                string fileName = $"Error_{DateTime.Now:yyyyMMdd}.log";
                string filePath = Path.Combine(logPath, fileName);

                string errorMessage = $@"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {ex.Message}
StackTrace: {ex.StackTrace}
InnerException: {ex.InnerException?.Message}
Source: {ex.Source}
URL: {Request.Url}
User: {User.Identity.Name}
---
";

                File.AppendAllText(filePath, errorMessage);
            }
            catch
            {
                // If logging fails, silently ignore to prevent infinite loop
            }
        }
    }
}
