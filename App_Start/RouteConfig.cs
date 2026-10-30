using System.Web.Mvc;
using System.Web.Routing;

namespace HRIS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Common", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "HRIS.Controllers" }
            ).DataTokens = new RouteValueDictionary(new { area = "Common" });
        }
    }
}