using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using puntosDeVenta.Domain.Entities.Sales;

namespace puntosDeVenta.Infrastructure.Persistence.Configurations
{
    public class SalesPointConfiguration : IEntityTypeConfiguration<SalesPoint>
    {
        public void Configure(EntityTypeBuilder<SalesPoint> builder)
        {
            builder.ToTable("puntosDeVenta", "sales");

            builder.HasKey(s => s.id);
            builder.Property(s => s.id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(s => s.code)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("code");

            builder.Property(s => s.name)
                   .IsRequired()
                   .HasMaxLength(250)
                   .HasColumnName("name");

            builder.Property(s => s.adress)
                   .HasMaxLength(500)
                   .HasColumnName("address");

            builder.Property(s => s.active)
                   .HasColumnName("active")
                   .HasDefaultValueSql("((1))");

            builder.Property(s => s.create)
                   .HasColumnName("create")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(s => s.update)
                   .HasColumnName("update")
                   .HasDefaultValueSql("GETDATE()");
        }
    }
}