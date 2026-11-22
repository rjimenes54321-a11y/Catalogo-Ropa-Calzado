using System.Collections.Generic;

namespace CatalogoRopaCalzado.ViewModels
{
    public class PanelVista
    {
        public int Productos { get; set; }
        public int Pendientes { get; set; }
        public int Ventas { get; set; }
        public int StockBajo { get; set; }
    }

    public class ReportesVista
    {
        public decimal VentasDia { get; set; }
        public decimal VentasMes { get; set; }
        public List<FilaReporte> MasVendidos { get; set; }
        public List<FilaReporte> ExistenciasBajas { get; set; }
    }

    public class FilaReporte
    {
        public string Nombre { get; set; }
        public long Cantidad { get; set; }
    }
}
