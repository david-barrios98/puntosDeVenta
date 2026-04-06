using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Outbound;
using System;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;

namespace puntosDeVenta.Infrastructure.Persistence.Repositories
{
    public class SaleRepository : SqlConfigServer
    {
        public SaleRepository(IConfiguration configuration) : base(configuration) { }

        public async Task<int> RegisterSaleAsync(RegisterSaleRequestDTO request)
        {
            using var connection = await OpenConnectionAsync();
            using var command = new SqlCommand("dbo.sp_RegistrarVentaPos", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@PosId", SqlDbType.NVarChar, 20) { Value = request.pos_id });
            command.Parameters.Add(new SqlParameter("@CashierId", SqlDbType.NVarChar, 20) { Value = request.cashier_id });
            command.Parameters.Add(new SqlParameter("@SaleDate", SqlDbType.DateTime2) { Value = request.sale_date });
            var totalParam = new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = request.total_amount };
            command.Parameters.Add(totalParam);

            var itemsJson = JsonSerializer.Serialize(request.items);
            command.Parameters.Add(new SqlParameter("@ItemsJson", SqlDbType.NVarChar, -1) { Value = itemsJson });

            var outParam = new SqlParameter("@VentaId", SqlDbType.Int) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outParam);

            await command.ExecuteNonQueryAsync();

            return (outParam.Value != DBNull.Value) ? Convert.ToInt32(outParam.Value) : 0;
        }
    }
}