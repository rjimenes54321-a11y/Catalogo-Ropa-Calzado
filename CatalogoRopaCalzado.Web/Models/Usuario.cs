using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoRopaCalzado.Models
{
    [Table("Usuarios", Schema = "dbo")]
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string ClaveHash { get; set; }
        public string Rol { get; set; }
        public int Intentos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
    }

    public static class RolesTienda
    {
        public const string Comprador = "Comprador";
        public const string Vendedor = "Vendedor";
        public const string Administrador = "Administrador";
        public const string Personal = Vendedor + "," + Administrador;
    }
}
