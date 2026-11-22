using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using CatalogoRopaCalzado.Data;
using CatalogoRopaCalzado.Models;
using CatalogoRopaCalzado.ViewModels;

namespace CatalogoRopaCalzado.Controllers
{
    [Permiso(Roles = RolesTienda.Personal)]
    public class PanelController : Controller
    {
        private readonly CatalogoContext db = new CatalogoContext();
        private const int UmbralStock = 5;
        private int OperadorId => int.Parse(User.Identity.Name);

        [HttpGet]
        public ActionResult Index() => View(new PanelVista
        {
            Productos = db.Productos.Count(p => p.Activo),
            Pendientes = db.Pedidos.Count(p => p.Estado == "Pendiente"),
            Ventas = db.Pedidos.Count(p => p.Estado == "Pagado"),
            StockBajo = db.Productos.Count(p => p.Activo && p.Stock <= UmbralStock)
        });

        [HttpGet]
        public ActionResult Pedidos() => View(db.Pedidos.OrderByDescending(p => p.Fecha).ToList());

        [HttpGet]
        public ActionResult Historial() => View("Pedidos", db.Pedidos.Where(p => p.Estado == "Pagado")
            .OrderByDescending(p => p.FechaPago).ToList());

        [HttpGet]
        public ActionResult Pedido(int id)
        {
            var pedido = db.Pedidos.Include(p => p.Detalles).SingleOrDefault(p => p.PedidoId == id);
            if (pedido == null) return HttpNotFound();
            ViewBag.FacturaId = db.Facturas.Where(f => f.PedidoId == id).Select(f => (int?)f.FacturaId).SingleOrDefault();
            return View(pedido);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Pago(int id)
        {
            TempData["Mensaje"] = new Ventas(db).ConfirmarPago(id, OperadorId)
                ? "Pago confirmado." : "Solo puedes confirmar el pago de un pedido pendiente.";
            return RedirectToAction("Pedido", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancelar(int id)
        {
            TempData["Mensaje"] = new Ventas(db).Cancelar(id)
                ? "Pedido cancelado. Se devolvieron las existencias." : "Solo puedes cancelar un pedido pendiente.";
            return RedirectToAction("Pedido", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Facturar(int id)
        {
            int? factura = new Ventas(db).Facturar(id, db.Usuarios.Find(OperadorId));
            if (factura.HasValue) return RedirectToAction("Factura", new { id = factura.Value });
            TempData["Mensaje"] = "Confirma el pago antes de generar la factura.";
            return RedirectToAction("Pedido", new { id });
        }

        [HttpGet]
        public ActionResult Facturacion() => View(db.Facturas.Include(f => f.Pedido).OrderByDescending(f => f.Fecha).ToList());

        [HttpGet]
        public ActionResult Factura(int id)
        {
            var factura = db.Facturas.Include(f => f.Pedido.Detalles).SingleOrDefault(f => f.FacturaId == id);
            return factura == null ? (ActionResult)HttpNotFound() : View(factura);
        }

        [HttpGet]
        public ActionResult Clientes() => View("Cuentas", db.Usuarios.Where(u => u.Rol == RolesTienda.Comprador)
            .OrderBy(u => u.Nombre).ToList());

        [Permiso(Roles = RolesTienda.Administrador), HttpGet]
        public ActionResult Usuarios() => View("Cuentas", db.Usuarios.OrderBy(u => u.Nombre).ToList());

        [HttpGet]
        public ActionResult Categorias() => View(db.Categorias.OrderBy(c => c.Nombre).ToList());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Categoria(int? id, string nombre)
        {
            nombre = (nombre ?? "").Trim();
            if (!ModelState.IsValid || nombre.Length == 0 || nombre.Length > 60)
            {
                TempData["Aviso"] = "Escribe un nombre de hasta 60 caracteres.";
                return RedirectToAction("Categorias");
            }

            var categoria = id.HasValue ? db.Categorias.Find(id.Value) : new Categoria();
            if (categoria == null) return HttpNotFound();
            if (!id.HasValue) db.Categorias.Add(categoria);
            categoria.Nombre = nombre;
            try
            {
                db.SaveChanges();
                TempData["Mensaje"] = "Categoría guardada.";
            }
            catch (DbUpdateException)
            {
                if (!db.Categorias.Any(c => c.Nombre == nombre && c.CategoriaId != categoria.CategoriaId)) throw;
                TempData["Aviso"] = "Ya existe una categoría con ese nombre.";
            }
            return RedirectToAction("Categorias");
        }

        [HttpGet]
        public ActionResult Reportes()
        {
            var zona = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zona);
            var dia = TimeZoneInfo.ConvertTimeToUtc(local.Date, zona);
            var mes = TimeZoneInfo.ConvertTimeToUtc(new DateTime(local.Year, local.Month, 1), zona);
            var pagados = db.Pedidos.Where(p => p.Estado == "Pagado");

            return View(new ReportesVista
            {
                VentasDia = pagados.Where(p => p.FechaPago >= dia).Sum(p => (decimal?)p.Total) ?? 0,
                VentasMes = pagados.Where(p => p.FechaPago >= mes).Sum(p => (decimal?)p.Total) ?? 0,
                MasVendidos = db.DetallePedidos.Where(d => pagados.Any(p => p.PedidoId == d.PedidoId))
                    .GroupBy(d => d.ProductoId)
                    .Select(g => new FilaReporte { Nombre = g.Max(d => d.Nombre), Cantidad = g.Sum(d => (long)d.Cantidad) })
                    .OrderByDescending(f => f.Cantidad).Take(10).ToList(),
                ExistenciasBajas = db.Productos.Where(p => p.Activo && p.Stock <= UmbralStock)
                    .OrderBy(p => p.Stock).ThenBy(p => p.Nombre)
                    .Select(p => new FilaReporte { Nombre = p.Nombre, Cantidad = p.Stock }).ToList()
            });
        }

        [HttpGet]
        public ActionResult Configuracion() => View();

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
