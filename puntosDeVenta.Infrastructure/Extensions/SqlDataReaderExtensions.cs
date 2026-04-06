using Microsoft.Data.SqlClient;
using System;

namespace Base.Api.Application.Extensions
{
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Obtiene el valor de la columna y maneja DBNull automáticamente
        /// </summary>
        public static T GetValue<T>(this SqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal))
                return default!;

            return (T)Convert.ChangeType(reader.GetValue(ordinal), typeof(T));
        }
    }
}
