using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAlmacen.Models
{
    public class HistorialStock
    {
        [Key]
        public int HistorialStockId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required, StringLength(50)]
        public string Codigo { get; set; }

        public string TipoEvento { get; set; } // Movimiento / Produccion

        public int? ReferenciaId { get; set; } // movimientoId o produccionId

        [Column(TypeName = "decimal(18,2)")]
        public decimal? StockAlmacenAntes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? StockAlmacenDespues { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? StockProduccionAntes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? StockProduccionDespues { get; set; }

        public string Usuario { get; set; }
        public string Observacion { get; set; }
    }
}
