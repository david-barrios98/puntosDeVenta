using Microsoft.EntityFrameworkCore;
using puntosDeVenta.Core.Domain.Entities;
using puntosDeVenta.Domain.Entities.Sales;
using puntosDeVenta.Infrastructure.Persistence.Adapters;
using System.Reflection;
using System.Text.Json;

namespace puntosDeVenta.Infrastructure.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(puntosDeVentaDbContext context)
        {
            await context.Database.MigrateAsync();

            // ⚠️ ORDEN IMPORTANTE (FK)
            //await SeedEntity<Productos>(context, "products.json");
        }

        // =============================
        // 🧠 GENERIC SEED
        // =============================
        private static async Task SeedEntity<T>(
            puntosDeVentaDbContext context,
            string fileName
        ) where T : class
        {
            var dbSet = context.Set<T>();

            // ✅ Evita duplicados
            if (dbSet.Any())
            {
                Console.WriteLine($"⚠️ {typeof(T).Name} ya tiene datos");
                return;
            }

            var data = await LoadJsonAsync<T>(fileName);

            if (data == null || !data.Any())
            {
                Console.WriteLine($"⚠️ {fileName} vacío");
                return;
            }

            try
            {
                dbSet.AddRange(data);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Seed {typeof(T).Name} insertado");
            }
            catch (Exception ex)
            {
                throw new Exception($"❌ Error en Seed de {typeof(T).Name}", ex);
            }
        }

        // =============================
        // 📦 JSON LOADER
        // =============================
        private static async Task<List<T>> LoadJsonAsync<T>(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(fileName));

            if (string.IsNullOrEmpty(resourceName))
                throw new Exception($"No se encontró el recurso JSON: {fileName}");

            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);

            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
        }
    }
}