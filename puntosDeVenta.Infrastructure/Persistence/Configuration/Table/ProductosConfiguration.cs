using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using puntosDeVenta.Core.Domain.Entities;
using puntosDeVenta.Domain.Entities.Sales;

namespace puntosDeVenta.Infrastructure.Persistence.Configurations
{
    public class ProductosConfiguration : IEntityTypeConfiguration<Productos>
    {
        public void Configure(EntityTypeBuilder<Productos> builder)
        {
            builder.ToTable("products", "catalog");

            builder.HasKey(p => p.producto_id);
            builder.Property(p => p.producto_id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(p => p.codigo_sku)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("code");

            builder.Property(p => p.nombre)
                   .IsRequired()
                   .HasMaxLength(250)
                   .HasColumnName("name");
        }
    }
}