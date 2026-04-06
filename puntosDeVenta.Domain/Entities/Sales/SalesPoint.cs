using puntosDeVenta.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace puntosDeVenta.Domain.Entities.Sales
{
    [Table("puntosDeVenta", Schema = "sales")]
    public class SalesPoint
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("code")]
        public string code { get; set; }

        [Required]
        [MaxLength(250)]
        [Column("name")]
        public string name { get; set; }

        [MaxLength(500)]
        [Column("address")]
        public string? adress { get; set; }

        [Column("active")]
        public bool active { get; set; } = true;

        [Column("create")]
        public DateTime create { get; set; } = DateTime.UtcNow;

        [Column("update")]
        public DateTime update { get; set; } = DateTime.UtcNow;
    }
}