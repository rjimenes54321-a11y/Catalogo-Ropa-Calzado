using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using CatalogoRopaCalzado.Models;

namespace CatalogoRopaCalzado.Data
{
    public class Ventas
    {
        private readonly CatalogoContext db;

        public Ventas(CatalogoContext contexto)
        {
            db = contexto;
        }

        public Pedido Crear(Usuario usuario, string direccion, string metodo, IDictionary<int, int> cantidades)
        {
            if (cantidades.Count == 0) throw new InvalidOperationException("El carrito está vacío.");

            using (var transaccion = db.Database.BeginTransaction())
            {
                var pedido = new Pedido
                {
                    CompradorId = usuario.UsuarioId, CompradorNombre = usuario.Nombre, CompradorCorreo = usuario.Correo,
                    Fecha = DateTime.UtcNow, Direccion = direccion, Metodo = metodo, Estado = "Pendiente"
                };

                // Un orden estable reduce la contención entre carritos con los mismos artículos.
                foreach (var linea in cantidades.OrderBy(l => l.Key))
                {
                    if (linea.Value < 1 || linea.Value > 99 ||
                        db.Database.ExecuteSqlCommand(
                            "UPDATE dbo.Productos SET Stock = Stock - @p0 WHERE ProductoId = @p1 AND Activo = 1 AND Precio > 0 AND Stock >= @p0",
                            linea.Value, linea.Key) != 1)
                    {
                        throw new InvalidOperationException("El carrito contiene un producto inactivo o con existencias insuficientes.");
                    }

                    var producto = db.Productos.AsNoTracking().Single(p => p.ProductoId == linea.Key);
                    pedido.Detalles.Add(new DetallePedido
                    {
                        ProductoId = producto.ProductoId, Nombre = producto.Nombre,
                        Cantidad = linea.Value, Precio = producto.Precio
                    });
                    pedido.Total += producto.Precio * linea.Value;
                }

                db.Pedidos.Add(pedido);
                db.SaveChanges();
                transaccion.Commit();
                return pedido;
            }
        }

        public bool ConfirmarPago(int id, int vendedorId)
        {
            return db.Database.ExecuteSqlCommand(
                "UPDATE dbo.Pedidos SET Estado = N'Pagado', FechaPago = SYSUTCDATETIME(), VendedorId = @p0 " +
                "WHERE PedidoId = @p1 AND Estado = N'Pendiente'", vendedorId, id) == 1;
        }

        public bool Cancelar(int id)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                if (db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.Pedidos SET Estado = N'Cancelado' WHERE PedidoId = @p0 AND Estado = N'Pendiente'", id) != 1)
                {
                    return false;
                }

                var lineas = db.DetallePedidos.Where(d => d.PedidoId == id).OrderBy(d => d.ProductoId).ToList();
                foreach (var linea in lineas)
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.Productos SET Stock = Stock + @p0 WHERE ProductoId = @p1",
                        linea.Cantidad, linea.ProductoId);
                }
                transaccion.Commit();
                return true;
            }
        }

        public int? Facturar(int id, Usuario vendedor)
        {
            using (var transaccion = db.Database.BeginTransaction())
            {
                var pedido = db.Pedidos.SqlQuery(
                    "SELECT * FROM dbo.Pedidos WITH (UPDLOCK, ROWLOCK) WHERE PedidoId = @p0", id).SingleOrDefault();
                if (pedido == null || pedido.Estado != "Pagado") return null;

                var factura = db.Facturas.SingleOrDefault(f => f.PedidoId == id);
                if (factura == null)
                {
                    factura = new Factura
                    {
                        PedidoId = id, Fecha = DateTime.UtcNow,
                        VendedorId = vendedor.UsuarioId, VendedorNombre = vendedor.Nombre
                    };
                    db.Facturas.Add(factura);
                    db.SaveChanges();
                }
                transaccion.Commit();
                return factura.FacturaId;
            }
        }
    }
}
