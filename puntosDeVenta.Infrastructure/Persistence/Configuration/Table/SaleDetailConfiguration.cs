using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using puntosDeVenta.Core.Domain.Entities.Sales;

namespace puntosDeVenta.Infrastructure.Persistence.Configurations
{
    public class SaleDetailConfiguration : IEntityTypeConfiguration<SaleDetail>
    {
        public void Configure(EntityTypeBuilder<SaleDetail> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.SaleHeaderId)
                .HasColumnName("sale_header_id")
                .IsRequired();

            builder.Property(x => x.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            builder.Property(x => x.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            builder.Property(x => x.UnitPrice)
                .HasColumnName("unit_price")
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(x => x.Subtotal)
                .HasColumnName("subtotal")
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETUTCDATE()");

            // �ndices
            builder.HasIndex(x => x.SaleHeaderId).HasDatabaseName("IDX_SaleHeaderId");
            builder.HasIndex(x => x.ProductId).HasDatabaseName("IDX_ProductId");

            // Relaci�n (FK)
            builder.HasOne(x => x.SaleHeader)
                .WithMany(x => x.SaleDetails)
                .HasForeignKey(x => x.SaleHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Schema y tabla
            builder.ToTable("sale_details", schema: "sales");
        }
    }
}