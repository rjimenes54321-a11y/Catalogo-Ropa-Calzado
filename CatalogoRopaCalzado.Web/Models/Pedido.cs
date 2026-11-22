using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoRopaCalzado.Models
{
    [Table("Pedidos", Schema = "dbo")]
    public class Pedido
    {
        public int PedidoId { get; set; }
        public int CompradorId { get; set; }
        public string CompradorNombre { get; set; }
        public string CompradorCorreo { get; set; }
        public DateTime Fecha { get; set; }
        public string Direccion { get; set; }
        public string Metodo { get; set; }
        public string Estado { get; set; }
        public decimal Total { get; set; }
        public int? VendedorId { get; set; }
        public DateTime? FechaPago { get; set; }
        public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();

        public string EstadoPago => Estado == "Pagado" ? "Confirmado" : Estado == "Cancelado" ? "Cancelado" : "Pendiente de pago";
        public string MetodoPago => Metodo == "Efectivo" ? "Efectivo contra entrega" : "Transferencia manual";
        public string EstadoPedido => Estado == "Cancelado" ? "Cancelado" : Estado == "Pagado" ? "Confirmado" : "Pendiente";
    }

    [Table("DetallePedidos", Schema = "dbo")]
    public class DetallePedido
    {
        public int DetallePedidoId { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }

    [Table("Facturas", Schema = "dbo")]
    public class Factura
    {
        public int FacturaId { get; set; }
        public int PedidoId { get; set; }
        public DateTime Fecha { get; set; }
        public int VendedorId { get; set; }
        public string VendedorNombre { get; set; }
        public Pedido Pedido { get; set; }
    }
}
