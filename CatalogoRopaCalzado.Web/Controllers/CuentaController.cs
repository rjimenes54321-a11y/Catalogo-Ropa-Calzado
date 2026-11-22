using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CatalogoRopaCalzado.Data;
using CatalogoRopaCalzado.Models;
using CatalogoRopaCalzado.ViewModels;

namespace CatalogoRopaCalzado.Controllers
{
    public class CuentaController : Controller
    {
        private readonly CatalogoContext db = new CatalogoContext();

        [HttpGet]
        public ActionResult Ingresar() => User.Identity.IsAuthenticated ? Destino() : View(new InicioSesion());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Ingresar(InicioSesion formulario)
        {
            if (!ModelState.IsValid) return View(formulario);
            string correo = formulario.Correo.Trim().ToLowerInvariant();
            using (var transaccion = db.Database.BeginTransaction())
            {
                var usuario = db.Usuarios.SqlQuery("SELECT * FROM dbo.Usuarios WITH (UPDLOCK, ROWLOCK) WHERE Correo = @p0", correo).SingleOrDefault();
                var ahora = DateTime.UtcNow;
                if (usuario != null && (!usuario.BloqueadoHasta.HasValue || usuario.BloqueadoHasta <= ahora))
                {
                    if (Claves.Verificar(formulario.Clave, usuario.ClaveHash))
                    {
                        usuario.Intentos = 0;
                        usuario.BloqueadoHasta = null;
                        db.SaveChanges();
                        transaccion.Commit();
                        Session.Clear();
                        FormsAuthentication.SetAuthCookie(usuario.UsuarioId.ToString(), false);
                        return usuario.Rol == RolesTienda.Comprador
                            ? RedirectToAction("Catalogo", "Tienda") : RedirectToAction("Index", "Panel");
                    }

                    usuario.Intentos++;
                    if (usuario.Intentos >= 5)
                    {
                        usuario.Intentos = 0;
                        usuario.BloqueadoHasta = ahora.AddMinutes(15);
                    }
                    db.SaveChanges();
                }
                transaccion.Commit();
            }

            ModelState.AddModelError("", "No se pudo iniciar sesión. Revisa tus datos o inténtalo más tarde.");
            return View(formulario);
        }

        [HttpGet]
        public ActionResult Registro() => View(new RegistroCuenta());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Registro([Bind(Exclude = "Rol")] RegistroCuenta formulario)
        {
            if (GuardarCuenta(formulario, RolesTienda.Comprador))
                return RedirectToAction("Ingresar");

            return View(formulario);
        }

        [Permiso(Roles = RolesTienda.Administrador), HttpGet]
        public ActionResult Privada() => View(new RegistroCuenta { Rol = RolesTienda.Vendedor });

        [Permiso(Roles = RolesTienda.Administrador), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Privada(RegistroCuenta formulario)
        {
            if (formulario.Rol != RolesTienda.Vendedor && formulario.Rol != RolesTienda.Administrador)
                ModelState.AddModelError("Rol", "Selecciona un rol privado válido.");

            if (GuardarCuenta(formulario, formulario.Rol))
                return RedirectToAction("Usuarios", "Panel");

            return View(formulario);
        }

        [Authorize, HttpGet]
        public ActionResult Perfil()
        {
            return View(db.Usuarios.Find(int.Parse(User.Identity.Name)));
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Salir()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Inicial", "Tienda");
        }

        private ActionResult Destino()
        {
            return User.IsInRole(RolesTienda.Comprador)
                ? RedirectToAction("Catalogo", "Tienda") : RedirectToAction("Index", "Panel");
        }

        private bool GuardarCuenta(RegistroCuenta formulario, string rol)
        {
            if (!Claves.EsValida(formulario.Clave))
                ModelState.AddModelError("Clave", "Usa entre 8 y 128 caracteres, con mayúscula, minúscula y número.");

            if (!ModelState.IsValid) return false;
            string correo = formulario.Correo.Trim().ToLowerInvariant();
            if (db.Usuarios.Any(u => u.Correo == correo))
            {
                ModelState.AddModelError("Correo", "Ese correo ya está registrado.");
                return false;
            }

            var usuario = new Usuario { Nombre = formulario.Nombre.Trim(), Correo = correo,
                ClaveHash = Claves.Crear(formulario.Clave), Rol = rol };
            db.Usuarios.Add(usuario);
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                if (!db.Usuarios.Any(u => u.Correo == correo)) throw;
                ModelState.AddModelError("Correo", "Ese correo ya está registrado.");
                return false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
