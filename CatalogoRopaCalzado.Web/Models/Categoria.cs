using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoRopaCalzado.Models
{
    [Table("Categorias", Schema = "dbo")]
    public class Categoria
    {
        public int CategoriaId { get; set; }

        [Required, StringLength(60)]
        public string Nombre { get; set; }
    }
}
