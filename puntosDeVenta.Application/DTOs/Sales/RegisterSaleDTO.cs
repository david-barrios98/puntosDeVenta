using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace puntosDeVenta.Application.DTOs.Sales
{
    public class RegisterSaleDTO
    {
        [Required]
        [MaxLength(20)]
        public string PosId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CashierId { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto total debe ser mayor a 0")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un item")]
        public List<SaleItemDTO> Items { get; set; } = new();
    }
}