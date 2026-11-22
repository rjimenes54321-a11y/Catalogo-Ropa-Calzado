using System.Collections.Generic;
using System.Web.Mvc;
using CatalogoRopaCalzado.Models;

namespace CatalogoRopaCalzado.ViewModels
{
    public class CatalogoVista
    {
        public string Buscar { get; set; }
        public int? CategoriaId { get; set; }
        public SelectList Categorias { get; set; }
        public IReadOnlyList<Producto> Productos { get; set; }
    }
}
