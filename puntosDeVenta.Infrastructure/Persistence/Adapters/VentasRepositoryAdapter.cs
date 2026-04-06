using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using puntosDeVenta.Application.DTOs.Sales;
using puntosDeVenta.Application.Ports.Outbound;

namespace puntosDeVenta.Infrastructure.Persistence.Adapters
{

    public class VentasRepositoryAdapter : SqlConfigServer, ISalesRepositoryAdapter
    {
        public VentasRepositoryAdapter(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<int> RegisterSaleAsync(RegisterSaleDTO sale)
        {
            var saleIdParameter = CreateOutputParameter("@SaleId", SqlDbType.Int);

            var itemsJson = JsonSerializer.Serialize(sale.Items.Select(i => new
            {
                i.producto_id,
                i.quantity,
                i.unit_price
            }).ToList());

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@PosId", SqlDbType.NVarChar, 20) { Value = sale.PosId },
                new SqlParameter("@CashierId", SqlDbType.NVarChar, 20) { Value = sale.CashierId },
                new SqlParameter("@SaleDate", SqlDbType.DateTime2) { Value = sale.SaleDate },
                new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = sale.TotalAmount, Precision = 12, Scale = 2 },
                new SqlParameter("@ItemsJson", SqlDbType.NVarChar, -1) { Value = itemsJson },
                saleIdParameter
            };

            await ExecuteStoredProcedureNonQueryAsync("sales.sp_RegistrarVentaPos", parameters);

            if (saleIdParameter.Value == DBNull.Value || !(saleIdParameter.Value is int))
            {
                throw new InvalidOperationException("No se pudo obtener el ID de la venta registrada");
            }

            return (int)saleIdParameter.Value;
        }
    }
}