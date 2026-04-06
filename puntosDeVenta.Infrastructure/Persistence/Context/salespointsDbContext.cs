using Microsoft.EntityFrameworkCore;
using puntosDeVenta.Core.Domain.Entities;
using puntosDeVenta.Domain.Entities.Sales;
using puntosDeVenta.Infrastructure.Persistence.Configurations;

namespace puntosDeVenta.Infrastructure.Persistence.Adapters
{
    public class puntosDeVentaDbContext : DbContext
    {
        public puntosDeVentaDbContext(DbContextOptions<puntosDeVentaDbContext> options)
            : base(options)
        {
        }
        public DbSet<Productos> Products { get; set; }
        public DbSet<SalesPoint> puntosDeVenta { get; set; }
        public DbSet<Product_puntosDeVenta> product_puntosDeVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductosConfiguration());
            modelBuilder.ApplyConfiguration(new SalesPointConfiguration());

            modelBuilder.ApplyConfiguration(new SaleHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new SaleDetailConfiguration());



            base.OnModelCreating(modelBuilder);
        }
    }
}