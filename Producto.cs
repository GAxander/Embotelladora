using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAlmacen.Models
{
    public class Producto
    {
        [Key]
        [StringLength(50)]
        public string Codigo { get; set; }   // PK -> coincide con la BD

        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; }

        [Required]
        [StringLength(100)]
        public string Familia { get; set; }

        [Required]
        [StringLength(50)]
        public string Linea { get; set; } // Soplado / Llenado

        [Required]
        [StringLength(20)]
        public string TipoMedida { get; set; } // Unidad / Kg

        [Column(TypeName = "decimal(18,2)")]
        public decimal StockAlmacen { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StockProduccion { get; set; }

        public ICollection<Movimiento> Movimientos { get; set; }
        public ICollection<DetalleProduccion> DetallesProduccion { get; set; }
    }
}
