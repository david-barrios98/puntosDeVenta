using puntosDeVenta.Core.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace puntosDeVenta.Domain.Entities.Sales
{
    [Table("sale_headers", Schema = "sales")]
    public class SaleHeader
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("pos_id")]
        public string PosId { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("cashier_id")]
        public string CashierId { get; set; }

        [Column("sale_date")]
        public DateTime SaleDate { get; set; }

        [Column("total_amount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Registered";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relaci�n
        [InverseProperty("SaleHeader")]
        public virtual ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }
}