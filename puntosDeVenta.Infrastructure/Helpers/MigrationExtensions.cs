using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace puntosDeVenta.Infrastructure.Helpers
{
    public static class MigrationExtensions
    {
        /// <summary>
        /// Crea o reemplaza uno o varios SPs desde recursos embebidos
        /// </summary>
        public static void CreateOrReplaceProcedures(
            this MigrationBuilder migrationBuilder,
            params (string FileName, string ProcedureName)[] procedures)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var allResourceNames = assembly.GetManifestResourceNames();

            foreach (var (fileName, procedureName) in procedures)
            {
                // Buscamos el recurso que CONTENGA el nombre del archivo
                // Esto evita errores por namespaces o carpetas
                var resourcePath = allResourceNames.FirstOrDefault(r => r.EndsWith(fileName));

                if (string.IsNullOrEmpty(resourcePath))
                    throw new Exception($"No se encontró el recurso embebido que termine en: {fileName}. " +
                                        "Asegúrate de que el archivo .sql tenga 'Build Action' como 'Embedded Resource'.");

                using var stream = assembly.GetManifestResourceStream(resourcePath);
                using var reader = new StreamReader(stream!);
                var sql = reader.ReadToEnd();

                // 1. Limpiamos si existe
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS {procedureName}");

                // 2. Creamos el procedimiento
                migrationBuilder.Sql(sql);
            }
        }
    }
}
