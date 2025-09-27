using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventarioAlmacen.Models
{
    public class Produccion
    {
        [Key]
        public int ProduccionId { get; set; }

        [StringLength(50)]
        public string NumeroReporte { get; set; }   // nvarchar(50) en la BD

        [Required]
        public DateTime Fecha { get; set; }

        public int? Turno { get; set; } // 1,2,3

        [Required, StringLength(50)]
        public string Linea { get; set; } // SOPLADO / LLENADO (selección limitada en la vista)

        [StringLength(50)]
        public string Formato { get; set; }  // SJ650ML, SJ2.5L, ...

        public int? OperadorId { get; set; }
        public Operador Operador { get; set; }

        public string Observacion { get; set; }

        public ICollection<DetalleProduccion> Detalles { get; set; } = new List<DetalleProduccion>();
    }
}
