using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using CatalogoRopaCalzado.Data;
using CatalogoRopaCalzado.Models;
using CatalogoRopaCalzado.ViewModels;

namespace CatalogoRopaCalzado.Controllers
{
    [Permiso(Roles = RolesTienda.Personal)]
    public class ProductosController : Controller
    {
        private readonly CatalogoContext db = new CatalogoContext();

        [HttpGet]
        public ActionResult Index(string buscar, int? categoriaId)
        {
            buscar = (buscar ?? "").Trim();
            if (buscar.Length > 120 || !ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400);
            }

            // IQueryable mantiene los filtros en SQL Server hasta ejecutar ToList().
            var consulta = db.Productos.AsNoTracking().Include(p => p.Categoria);
            if (buscar.Length > 0)
            {
                consulta = consulta.Where(p => p.Nombre.Contains(buscar));
            }

            if (categoriaId.HasValue)
            {
                consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);
            }

            return View(new CatalogoVista
            {
                Buscar = buscar,
                CategoriaId = categoriaId,
                Categorias = ObtenerCategorias(categoriaId),
                Productos = consulta.OrderBy(p => p.Nombre).ThenBy(p => p.ProductoId).ToList()
            });
        }

        [HttpGet]
        public ActionResult Crear()
        {
            return MostrarFormulario("Crear", new ProductoFormulario());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Crear([Bind(Include = "Codigo,Nombre,Descripcion,Imagen,Activo,Talla,CategoriaId,Precio,Stock")] ProductoFormulario formulario)
        {
            if (!ValidarFormulario(formulario))
            {
                return MostrarFormulario("Crear", formulario);
            }

            var producto = new Producto();
            AplicarFormulario(producto, formulario);
            db.Productos.Add(producto);
            return Guardar("Crear", formulario, "Producto creado.");
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            return MostrarFormulario("Editar", new ProductoFormulario
            {
                ProductoId = producto.ProductoId,
                Codigo = producto.Codigo,
                Descripcion = producto.Descripcion,
                Imagen = producto.Imagen,
                Activo = producto.Activo,
                StockOriginal = producto.Stock,
                Nombre = producto.Nombre,
                Talla = producto.Talla,
                CategoriaId = producto.CategoriaId,
                Precio = producto.Precio,
                Stock = producto.Stock
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Editar(int id, [Bind(Include = "Codigo,Nombre,Descripcion,Imagen,Activo,Talla,CategoriaId,Precio,Stock,StockOriginal")] ProductoFormulario formulario)
        {
            var producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            formulario.ProductoId = id;
            if (producto.Stock != formulario.StockOriginal)
            {
                ModelState.AddModelError("", "Las existencias cambiaron. Vuelve a abrir el formulario antes de editar.");
            }
            if (!ValidarFormulario(formulario))
            {
                return MostrarFormulario("Editar", formulario);
            }

            AplicarFormulario(producto, formulario);
            return Guardar("Editar", formulario, "Producto actualizado.");
        }

        [HttpGet]
        public ActionResult Eliminar(int id)
        {
            var producto = db.Productos.AsNoTracking().Include(p => p.Categoria)
                .SingleOrDefault(p => p.ProductoId == id);
            return producto == null ? (ActionResult)HttpNotFound() : View(producto);
        }

        [HttpPost, ActionName("Eliminar"), ValidateAntiForgeryToken]
        public ActionResult ConfirmarEliminacion(int id)
        {
            var producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            producto.Activo = false;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Aviso"] = "Las existencias cambiaron. Vuelve a intentar la desactivación.";
                return RedirectToAction("Eliminar", new { id });
            }

            TempData["Mensaje"] = "Producto desactivado.";
            return RedirectToAction("Index");
        }

        private bool ValidarFormulario(ProductoFormulario formulario)
        {
            if (ModelState.IsValid && db.Productos.Any(p => p.Codigo == formulario.Codigo.Trim()
                && p.ProductoId != formulario.ProductoId))
            {
                ModelState.AddModelError("Codigo", "Ya existe un producto con ese código.");
            }
            if (ModelState.IsValid && !db.Categorias.Any(c => c.CategoriaId == formulario.CategoriaId.Value))
            {
                ModelState.AddModelError(nameof(formulario.CategoriaId), "La categoría seleccionada no existe.");
            }

            return ModelState.IsValid;
        }

        private SelectList ObtenerCategorias(int? seleccion)
        {
            return new SelectList(db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToList(),
                "CategoriaId", "Nombre", seleccion);
        }

        private ViewResult MostrarFormulario(string vista, ProductoFormulario formulario)
        {
            formulario.Categorias = ObtenerCategorias(formulario.CategoriaId);
            return View(vista, formulario);
        }

        private static void AplicarFormulario(Producto producto, ProductoFormulario formulario)
        {
            producto.Codigo = formulario.Codigo.Trim();
            producto.Descripcion = formulario.Descripcion?.Trim();
            producto.Imagen = string.IsNullOrWhiteSpace(formulario.Imagen) ? null : formulario.Imagen.Trim();
            producto.Activo = formulario.Activo;
            producto.Nombre = formulario.Nombre.Trim();
            producto.Talla = string.IsNullOrWhiteSpace(formulario.Talla) ? null : formulario.Talla.Trim();
            producto.CategoriaId = formulario.CategoriaId.Value;
            producto.Precio = formulario.Precio.Value;
            producto.Stock = formulario.Stock.Value;
        }

        private ActionResult Guardar(string vista, ProductoFormulario formulario, string mensaje)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Las existencias cambiaron. Vuelve a abrir el formulario antes de editar.");
                return MostrarFormulario(vista, formulario);
            }
            catch (DbUpdateException)
            {
                if (!db.Productos.Any(p => p.Codigo == formulario.Codigo.Trim() && p.ProductoId != formulario.ProductoId)) throw;
                ModelState.AddModelError("Codigo", "Ya existe un producto con ese código.");
                return MostrarFormulario(vista, formulario);
            }

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Index");
        }

        protected override void OnException(ExceptionContext contexto)
        {
            if (contexto.Exception is HttpAntiForgeryException)
            {
                ResponderError(contexto, 400, "El formulario venció o no es válido. Vuelve a abrirlo e inténtalo de nuevo.");
            }
            else if (contexto.Exception is DataException || contexto.Exception is SqlException)
            {
                Trace.TraceError(contexto.Exception.ToString());
                ResponderError(contexto, 503, "El catálogo no pudo completar la operación. Inténtalo de nuevo.");
            }
        }

        private void ResponderError(ExceptionContext contexto, int codigo, string mensaje)
        {
            Response.StatusCode = codigo;
            Response.TrySkipIisCustomErrors = true;
            contexto.Result = View("Error", (object)mensaje);
            contexto.ExceptionHandled = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
