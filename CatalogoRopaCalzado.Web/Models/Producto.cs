using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoRopaCalzado.Models
{
    [Table("Productos", Schema = "dbo")]
    public class Producto
    {
        public int ProductoId { get; set; }

        [Required, StringLength(40)]
        public string Codigo { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [StringLength(500)]
        public string Imagen { get; set; }

        public bool Activo { get; set; }

        [Required, StringLength(120)]
        public string Nombre { get; set; }

        [StringLength(30)]
        public string Talla { get; set; }

        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; }
        public decimal Precio { get; set; }
        [ConcurrencyCheck]
        public int Stock { get; set; }
    }
}
