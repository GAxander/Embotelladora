using System.ComponentModel.DataAnnotations;

namespace InventarioAlmacen.Models
{
    public class Operador
    {
        [Key]
        public int OperadorId { get; set; }

        [Required, StringLength(150)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Area { get; set; } // Soplado / Llenado

        
    }
}
