using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using puntosDeVenta.Core.Domain.Entities.Sales;
using puntosDeVenta.Domain.Entities.Sales;

namespace puntosDeVenta.Infrastructure.Persistence.Configurations
{
    public class SaleHeaderConfiguration : IEntityTypeConfiguration<SaleHeader>
    {
        public void Configure(EntityTypeBuilder<SaleHeader> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.PosId)
                .HasColumnName("pos_id")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CashierId)
                .HasColumnName("cashier_id")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.SaleDate)
                .HasColumnName("sale_date")
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(12, 2)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .HasDefaultValue("Registered");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("GETUTCDATE()");

            // �ndices
            builder.HasIndex(x => x.PosId).HasDatabaseName("IDX_PosId");
            builder.HasIndex(x => x.SaleDate).HasDatabaseName("IDX_SaleDate");
            builder.HasIndex(x => x.Status).HasDatabaseName("IDX_Status");

            // Relaciones
            builder.HasMany(x => x.SaleDetails)
                .WithOne(x => x.SaleHeader)
                .HasForeignKey(x => x.SaleHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Schema y tabla
            builder.ToTable("sale_headers", schema: "sales");
        }
    }
}