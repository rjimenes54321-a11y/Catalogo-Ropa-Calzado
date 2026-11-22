using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CatalogoRopaCalzado.Data;
using CatalogoRopaCalzado.Models;
using CatalogoRopaCalzado.ViewModels;

namespace CatalogoRopaCalzado.Controllers
{
    public class TiendaController : Controller
    {
        private readonly CatalogoContext db = new CatalogoContext();
        private int CompradorId => int.Parse(User.Identity.Name);

        private Dictionary<int, int> CarritoActual
        {
            get
            {
                var carrito = Session["Carrito"] as Dictionary<int, int>;
                if (carrito != null) return carrito;
                carrito = new Dictionary<int, int>();
                Session["Carrito"] = carrito;
                return carrito;
            }
        }

        [HttpGet]
        public ActionResult Inicial() => View();

        [HttpGet]
        public ActionResult Catalogo(string buscar, int? categoriaId)
        {
            buscar = (buscar ?? "").Trim();
            if (buscar.Length > 120 || !ModelState.IsValid) return new HttpStatusCodeResult(400);
            var consulta = db.Productos.AsNoTracking().Include(p => p.Categoria).Where(p => p.Activo);
            if (buscar.Length > 0) consulta = consulta.Where(p => p.Nombre.Contains(buscar));
            if (categoriaId.HasValue) consulta = consulta.Where(p => p.CategoriaId == categoriaId.Value);

            return View(new CatalogoVista
            {
                Buscar = buscar, CategoriaId = categoriaId,
                Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre).ToList(), "CategoriaId", "Nombre", categoriaId),
                Productos = consulta.OrderBy(p => p.Nombre).ToList()
            });
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            var producto = db.Productos.Include(p => p.Categoria).SingleOrDefault(p => p.ProductoId == id && p.Activo);
            return producto == null ? (ActionResult)HttpNotFound() : View(producto);
        }

        [Permiso(Roles = RolesTienda.Comprador), HttpGet]
        public ActionResult Carrito() => View(CargarCarrito(new CompraVista()));

        [Permiso(Roles = RolesTienda.Comprador), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Agregar(int id, int cantidad)
        {
            int anterior;
            CarritoActual.TryGetValue(id, out anterior);
            if (cantidad < 1 || cantidad > 99 - anterior)
            {
                TempData["Aviso"] = "Selecciona una cantidad válida.";
                return RedirectToAction("Detalle", new { id });
            }
            return Ajustar(id, anterior + cantidad);
        }

        [Permiso(Roles = RolesTienda.Comprador), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cantidad(int id, int cantidad) => Ajustar(id, cantidad);

        [Permiso(Roles = RolesTienda.Comprador), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Quitar(int id)
        {
            CarritoActual.Remove(id);
            return RedirectToAction("Carrito");
        }

        [Permiso(Roles = RolesTienda.Comprador), HttpPost, ValidateAntiForgeryToken]
        public ActionResult Confirmar([Bind(Include = "Direccion,Metodo")] CompraVista compra)
        {
            if (!ModelState.IsValid) return View("Carrito", CargarCarrito(compra));
            try
            {
                var pedido = new Ventas(db).Crear(db.Usuarios.Find(CompradorId), compra.Direccion.Trim(), compra.Metodo, CarritoActual);
                CarritoActual.Clear();
                TempData["Mensaje"] = "Pedido registrado y pendiente de pago.";
                return RedirectToAction("Pedido", new { id = pedido.PedidoId });
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("", "Verifica que el carrito tenga productos activos con existencias suficientes.");
                return View("Carrito", CargarCarrito(compra));
            }
        }

        [Permiso(Roles = RolesTienda.Comprador), HttpGet]
        public ActionResult Pedidos() => View(db.Pedidos.Where(p => p.CompradorId == CompradorId)
            .OrderByDescending(p => p.Fecha).ToList());

        [Permiso(Roles = RolesTienda.Comprador), HttpGet]
        public ActionResult Pedido(int id)
        {
            var pedido = db.Pedidos.Include(p => p.Detalles).SingleOrDefault(p => p.PedidoId == id && p.CompradorId == CompradorId);
            return pedido == null ? (ActionResult)HttpNotFound() : View(pedido);
        }

        private ActionResult Ajustar(int id, int cantidad)
        {
            var producto = db.Productos.Find(id);
            if (producto == null || !producto.Activo || cantidad < 1 || cantidad > 99 ||
                cantidad > producto.Stock || (!CarritoActual.ContainsKey(id) && CarritoActual.Count >= 100))
            {
                TempData["Aviso"] = "La cantidad no está disponible. Verifica el producto y las existencias.";
            }
            else
            {
                CarritoActual[id] = cantidad;
            }
            return RedirectToAction("Carrito");
        }

        private CompraVista CargarCarrito(CompraVista compra)
        {
            var ids = CarritoActual.Keys.ToArray();
            var productos = db.Productos.Where(p => ids.Contains(p.ProductoId)).OrderBy(p => p.Nombre).ToList();
            compra.Lineas = productos.Select(p => new LineaCarrito
            {
                Producto = p, Cantidad = CarritoActual[p.ProductoId]
            }).ToList();
            foreach (var id in ids.Except(productos.Select(p => p.ProductoId))) CarritoActual.Remove(id);
            return compra;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
