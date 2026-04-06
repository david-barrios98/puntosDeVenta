using puntosDeVenta.Domain.Entities.Sales;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace puntosDeVenta.Core.Domain.Entities.Sales
{
    [Table("sale_details", Schema = "sales")]
    public class SaleDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("sale_header_id")]
        public int SaleHeaderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price", TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; }

        [Column("subtotal", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relaci�n
        [ForeignKey("SaleHeaderId")]
        [InverseProperty("SaleDetails")]
        public virtual SaleHeader SaleHeader { get; set; }
    }
}