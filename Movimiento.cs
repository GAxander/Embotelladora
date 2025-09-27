using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioAlmacen.Models
{
    public class Movimiento
    {
        public int MovimientoId { get; set; }

        [Required]
        public string Codigo { get; set; } = string.Empty;

        [ForeignKey("Codigo")]
        public Producto? Producto { get; set; }  // <-- quitar Required aquí

        [Required]
        public string TipoMovimiento { get; set; } = string.Empty;

        [Required]
        public decimal Cantidad { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaMovimiento { get; set; }


        public string? NumeroDocumento { get; set; }

        public string? Observacion { get; set; }
        [NotMapped]
        public decimal StockAlmacenAntes { get; set; }
        [NotMapped]
        public decimal StockAlmacenDespues { get; set; }
        [NotMapped]
        public decimal StockProduccionAntes { get; set; }
        [NotMapped]
        public decimal StockProduccionDespues { get; set; }
    }
}
