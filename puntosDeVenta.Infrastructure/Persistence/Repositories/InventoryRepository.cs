using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using puntosDeVenta.Application.DTOs.Inventory;
using puntosDeVenta.Application.Ports.Outbound;
using puntosDeVenta.Infrastructure.Constants;
using puntosDeVenta.Infrastructure.Persistence.Adapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace puntosDeVenta.Infrastructure.Persistence.Repositories
{
    public class InventoryRepository : SqlConfigServer, IInventoryRepository
    {
        public InventoryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<IEnumerable<ProductoCriticalDTO>> GetCriticalProductsAsync(int salesPointId)
        {
            try
            {
                var sqlParams = new SqlParameter[]
{
                CreateParameter("@Point_Id", salesPointId, SqlDbType.Int)
};

                var list = await ExecuteStoredProcedureAsync(
                    StoredProcedures.Inventary.sp_consultarProductosCriticos,
                    sqlParams,
                    reader =>
                    {
                        return new ProductoCriticalDTO
                        {
                            Producto_Id = reader.GetInt32(0),
                            Codigo_Sku = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                            Nombre = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Stock_Actual = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                            Stock_Minimo_Permitido = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4)
                        };
                    });

                return list;
            }
            catch (Exception e)
            {
                return new List<ProductoCriticalDTO>();
            }

        }

        public async Task<bool> PosExistsAsync(int salesPointId)
        {
            try
            {
                using var connection = await OpenConnectionAsync();
                using var command = new SqlCommand("SELECT TOP 1 1 FROM Inventario_POS WHERE pos_id = @Point_Id", connection)
                {
                    CommandType = CommandType.Text
                };

                command.Parameters.Add(CreateParameter("@Point_Id", salesPointId, SqlDbType.Int));

                var result = await command.ExecuteScalarAsync();

                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}