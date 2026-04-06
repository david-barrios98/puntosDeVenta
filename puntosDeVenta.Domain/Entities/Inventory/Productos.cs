using puntosDeVenta.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace puntosDeVenta.Core.Domain.Entities
{
    [Table("productos", Schema = "dbo")]
    public class Productos
    {
        [Key]
        [Column("producto_id")]
        public int producto_id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("codigo_sku")]
        public string codigo_sku { get; set; }

        [Required]
        [MaxLength(250)]
        [Column("nombre")]
        public string nombre { get; set; }
    }
}