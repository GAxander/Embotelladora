using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAlmacen.Models
{
    public class DetalleProduccion
    {
        public int DetalleProduccionId { get; set; }
        public int ProduccionId { get; set; }
        public string TipoDetalle { get; set; } // SOPLADO = PROD, MERM, ING1 | LLENADO = PROD, MERM
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public int? UnidadPorPaquete { get; set; }
        public int? Paquetes { get; set; }

        public Produccion Produccion { get; set; }
        public Producto Producto { get; set; }
    }
}
