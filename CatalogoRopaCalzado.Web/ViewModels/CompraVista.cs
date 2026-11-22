using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CatalogoRopaCalzado.Models;

namespace CatalogoRopaCalzado.ViewModels
{
    public class CompraVista
    {
        public List<LineaCarrito> Lineas { get; set; } = new List<LineaCarrito>();
        public bool Disponible => Lineas.Count > 0 && Lineas.All(l => l.Disponible);
        public decimal Total => Lineas.Sum(l => l.Subtotal);

        [Required(ErrorMessage = "Escribe la dirección de entrega."), StringLength(300)]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "Selecciona un método de pago.")]
        [RegularExpression("Efectivo|Transferencia", ErrorMessage = "Método de pago no válido.")]
        public string Metodo { get; set; }
    }

    public class LineaCarrito
    {
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public bool Disponible => Producto.Activo && Producto.Stock >= Cantidad;
        public decimal Subtotal => Producto.Precio * Cantidad;
    }
}
