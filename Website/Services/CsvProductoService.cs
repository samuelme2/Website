using TuProyecto.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TuProyecto.Services
{
    public static class CsvProductoService
    {
        // Ruta relativa al archivo CSV (ajusta si es necesario)
        private const string CsvFilePath = "Data/productos_detallados_con_precio.csv";

        public static List<Producto> LoadProductos(string webRootPath)
        {
            var fullPath = Path.Combine(webRootPath, CsvFilePath);
            var productos = new List<Producto>();
            int idCounter = 1; // Para asignar IDs únicos

            if (!File.Exists(fullPath))
            {
                // Devolver una lista vacía si el archivo no existe
                return productos;
            }

            // Usar File.ReadLines para leer línea por línea
            // .Skip(1) es para omitir la fila de encabezados
            var lines = File.ReadLines(fullPath).Skip(1);

            foreach (var line in lines)
            {
                // ATENCIÓN: Los CSV pueden usar , o ; como separador. 
                // Asegúrate de usar el separador correcto aquí. Asumiré la coma (,)
                var values = line.Split(',');

                // Mapeo de columnas (asumiendo el orden del script Python: Nombre, Precio, URLs_Imagenes, Detalles_o_Descripcion, URL_Producto)
                // ¡Asegúrate de que este mapeo coincida con el orden de tu CSV real!
                if (values.Length >= 5) // Ahora esperamos al menos 5 columnas
                {
                    var nombre = values[0].Trim();
                    var precioStr = values[1].Trim(); // El precio limpio
                    var imagenUrlCompleta = values[2].Trim();
                    var descripcion = values[3].Trim();
                    var urlProducto = values[4].Trim();

                    // 1. Obtener la imagen principal
                    var imagenUrlPrincipal = imagenUrlCompleta.Split('|').FirstOrDefault() ?? "/images/default.jpg";

                    // 2. Parsear el precio
                    if (!decimal.TryParse(precioStr, System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.InvariantCulture, out decimal precio))
                    {
                        // Usa CultureInfo.InvariantCulture porque el Python script genera un punto (.) como separador decimal.
                        precio = 0.00m;
                    }
                    var imagenes = imagenUrlCompleta.Split('|').Select(url => url.Trim()).Where(url => !string.IsNullOrEmpty(url)).ToList();

                    productos.Add(new Producto
                    {
                        Id = idCounter++,
                        Nombre = nombre,
                        Categoria = "Juguetes",
                        Precio = precio,
                        ImagenUrl = imagenes,
                        Descripcion = descripcion
                    });
                }
            }

            return productos;
        }
    }
}