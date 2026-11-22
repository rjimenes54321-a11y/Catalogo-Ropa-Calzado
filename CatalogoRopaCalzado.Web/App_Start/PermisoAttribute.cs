using System.Web.Mvc;

namespace CatalogoRopaCalzado
{
    public sealed class PermisoAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext contexto)
        {
            if (!contexto.HttpContext.User.Identity.IsAuthenticated)
            {
                base.HandleUnauthorizedRequest(contexto);
                return;
            }

            var respuesta = contexto.HttpContext.Response;
            respuesta.StatusCode = 403;
            respuesta.TrySkipIisCustomErrors = true;
            contexto.Result = new ViewResult
            {
                ViewName = "~/Views/Shared/Error.cshtml",
                ViewData = new ViewDataDictionary("Tu cuenta no tiene acceso a esta sección.")
            };
        }
    }
}
