namespace puntosDeVenta.Application.DTOs.Inventory
{
    public class ProductoCriticalDTO
    {
        public int Producto_Id { get; set; }
        public string Codigo_Sku { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public Decimal Stock_Actual { get; set; }
        public Decimal Stock_Minimo_Permitido { get; set; }
    }
}