using System;
using System.Collections.Generic;

namespace puntosDeVenta.Application.DTOs.Sales
{
    public class SaleItemDTO
    {
        public int producto_id { get; set; }
        public int quantity { get; set; }
        public decimal unit_price { get; set; }
    }

    public class RegisterSaleRequestDTO
    {
        public string pos_id { get; set; } = string.Empty;
        public string cashier_id { get; set; } = string.Empty;
        public DateTime sale_date { get; set; }
        public decimal total_amount { get; set; }
        public List<SaleItemDTO> items { get; set; } = new();
    }

    public class RegisterSaleResponseDTO
    {
        public string status { get; set; } = "success";
        public int internal_sale_id { get; set; }
        public string message { get; set; } = "Venta registrada exitosamente";
        public DateTime processed_at { get; set; }
    }
}