using System;
using System.Data.SqlClient;
using System.Reflection;
using Base.Api.Application.Extensions;
using Microsoft.Data.SqlClient;

namespace puntosDeVenta.Infrastructure.Extensions
{
    public static class SqlDataReaderMapper
    {
        public static T MapToDto<T>(SqlDataReader reader) where T : new()
        {
            var dto = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                if (HasColumn(reader, prop.Name))
                {
                    var value = reader.GetValue<object>(prop.Name); // usa tu extensión
                    if (value != null)
                        prop.SetValue(dto, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            return dto;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}