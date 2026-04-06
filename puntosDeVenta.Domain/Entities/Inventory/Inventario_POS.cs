using puntosDeVenta.Core.Domain.Entities;
using puntosDeVenta.Domain.Entities.Sales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Inventario_POS", Schema = "dbo")]
public class Product_puntosDeVenta
{
    [Key]
    [Column("pos_id")]
    public int pos_id { get; set; }

    [Column("producto_id")]
    public int producto_id { get; set; }

    [Required]
    [Column("stock_actual")]
    public int stock_actual { get; set; }

    [Required]
    [Column("stock_minomo_permitido")]
    public int stock_minomo_permitido { get; set; }

    // Propiedades de Navegaci�n con llaves for�neas expl�citas
    [ForeignKey("producto_id")]
    public Productos? Producto { get; set; }
}