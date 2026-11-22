using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Principal;
using System.Threading;
using CatalogoRopaCalzado.Data;

namespace CatalogoRopaCalzado
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_PostAuthenticateRequest()
        {
            if (Context.User?.Identity?.IsAuthenticated != true) return;

            int id;
            if (int.TryParse(Context.User.Identity.Name, out id))
            {
                using (var db = new CatalogoContext())
                {
                    var usuario = db.Usuarios.Find(id);
                    if (usuario != null)
                    {
                        Context.User = new GenericPrincipal(Context.User.Identity, new[] { usuario.Rol });
                        Thread.CurrentPrincipal = Context.User;
                        return;
                    }
                }
            }

            Context.User = new GenericPrincipal(new GenericIdentity(""), new string[0]);
            Thread.CurrentPrincipal = Context.User;
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
