using System.Web.Mvc;
using System.Web.Routing;

namespace CatalogoRopaCalzado
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("Inicio", "", new { controller = "Tienda", action = "Inicial" });
            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new { controller = "Tienda", action = "Index", id = UrlParameter.Optional });
        }
    }
}
