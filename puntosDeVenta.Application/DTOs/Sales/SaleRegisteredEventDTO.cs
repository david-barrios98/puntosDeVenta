using System;

namespace puntosDeVenta.Application.DTOs.Sales
{
    public class SaleRegisteredEventDTO
    {
        public string EventType { get; set; } = "SaleRegistered";
        public int InternalSaleId { get; set; }
        public string PosId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}