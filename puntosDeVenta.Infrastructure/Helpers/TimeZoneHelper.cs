using System;

namespace puntosDeVenta.Shared.Helper
{
    public static class TimeZoneHelper
    {
        // Zona horaria de Colombia (UTC-5)
        private static readonly TimeZoneInfo ColombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");

        /// <summary>
        /// Obtiene la hora actual en zona horaria de Colombia
        /// </summary>
        public static DateTime GetColombiaTime()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, ColombiaTimeZone);
        }

        /// <summary>
        /// Obtiene la hora actual en zona horaria de Colombia (con tipo DateTime)
        /// </summary>
        public static DateTime GetColombiaTimeNow()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, ColombiaTimeZone);
        }

        /// <summary>
        /// Convierte una fecha UTC a zona horaria de Colombia
        /// </summary>
        public static DateTime ConvertToColombiaTime(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTime(utcTime, ColombiaTimeZone);
        }

        /// <summary>
        /// Convierte una fecha de Colombia a UTC
        /// </summary>
        public static DateTime ConvertToUtc(DateTime colombiaTime)
        {
            return TimeZoneInfo.ConvertTime(colombiaTime, ColombiaTimeZone, TimeZoneInfo.Utc);
        }

        /// <summary>
        /// Obtiene el offset de zona horaria de Colombia
        /// </summary>
        public static TimeSpan GetColombiaOffset()
        {
            return ColombiaTimeZone.GetUtcOffset(DateTime.Now);
        }

        /// <summary>
        /// Obtiene informaci�n de zona horaria
        /// </summary>
        public static (string DisplayName, string StandardName, TimeSpan Offset) GetColombiaTimeZoneInfo()
        {
            return (ColombiaTimeZone.DisplayName, ColombiaTimeZone.StandardName, GetColombiaOffset());
        }
    }
}