using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CatalogoRopaCalzado.ViewModels
{
    public class ProductoFormulario : IValidatableObject
    {
        public int ProductoId { get; set; }
        public int StockOriginal { get; set; }

        [Required(ErrorMessage = "Escribe un código de producto."), StringLength(40)]
        public string Codigo { get; set; }

        [StringLength(500)]
        public string Descripcion { get; set; }

        [StringLength(500)]
        public string Imagen { get; set; }

        public bool Activo { get; set; } = true;

        [Required(ErrorMessage = "Escribe el nombre del producto.")]
        [StringLength(120, ErrorMessage = "El nombre admite hasta 120 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(30, ErrorMessage = "La talla admite hasta 30 caracteres.")]
        public string Talla { get; set; }

        [Display(Name = "Categoría")]
        [Required(ErrorMessage = "Selecciona una categoría.")]
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        public int? CategoriaId { get; set; }

        [Display(Name = "Precio (USD)")]
        [Required(ErrorMessage = "Escribe el precio.")]
        public decimal? Precio { get; set; }

        [Display(Name = "Existencias")]
        [Required(ErrorMessage = "Escribe las existencias.")]
        [Range(0, int.MaxValue, ErrorMessage = "Las existencias deben ser un entero no negativo.")]
        public int? Stock { get; set; }

        public SelectList Categorias { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Precio.HasValue &&
                (Precio.Value <= 0m || Precio.Value > 9999999999.99m || decimal.Round(Precio.Value, 2) != Precio.Value))
            {
                yield return new ValidationResult(
                    "Escribe un precio positivo con hasta dos decimales y un máximo de 9,999,999,999.99.",
                    new[] { nameof(Precio) });
            }
            if (!string.IsNullOrWhiteSpace(Imagen) && !EsImagenSegura(Imagen.Trim()))
                yield return new ValidationResult("Escribe una dirección HTTPS válida para la imagen.", new[] { nameof(Imagen) });
        }

        private static bool EsImagenSegura(string imagen)
        {
            System.Uri uri;
            return System.Uri.TryCreate(imagen, System.UriKind.Absolute, out uri)
                && uri.Scheme == System.Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.UserInfo);
        }
    }
}
