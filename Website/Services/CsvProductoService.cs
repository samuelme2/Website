using TuProyecto.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System;

namespace TuProyecto.Services
{
    public static class CsvProductoService
    {
        private const string CsvFilePath = "Data/productos_detallados_con_precio.csv";

        // Definición de subfiltros que pertenecen a la categoría principal "Juguetes"
        private static readonly HashSet<string> JUGUETES_SUBFILTERS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Dildos", "Vibradores", "Torsos", "Anales", "Sex Machine",
            "Lubricantes", "Bienestar Sexual", "Bondage", "Kits"
        };

        public static List<Producto> LoadProductos(string webRootPath)
        {
            var fullPath = Path.Combine(webRootPath, CsvFilePath);
            var productos = new List<Producto>();
            int idCounter = 1;

            if (!File.Exists(fullPath))
            {
                return productos;
            }

            try
            {
                var lines = File.ReadLines(fullPath).Skip(1);

                foreach (var line in lines)
                {
                    // Se asume la coma (,) como separador
                    var values = line.Split(',');

                    // 0: Nombre, 1: Precio, 2: Categorias (del scraper), 3: URLs_Imagenes, 4: Descripcion_Extendida, 5: URL_Producto
                    if (values.Length >= 6)
                    {
                        var nombre = values[0].Trim();
                        var precioStr = values[1].Trim();
                        var categoriaCsv = values[2].Trim();
                        var imagenUrlCompleta = values[3].Trim();
                        var descripcion = values[4].Trim();
                        // var urlProducto = values[5].Trim();

                        // 1. Procesar la columna de Categorias/Subfiltros del CSV
                        var subFiltros = categoriaCsv
                            .Split('|')
                            .Select(c => c.Trim())
                            .Where(c => !string.IsNullOrEmpty(c))
                            .ToList();

                        // 2. Determinar la Categoria Principal: "Lovense" o "Juguetes"
                        var categoriaPrincipal = "Lovense"; // Valor por defecto

                        // Si ALGUNO de los subfiltros coincide con la lista de JUGUETES_SUBFILTERS, 
                        // el producto se categoriza como "Juguetes".
                        if (subFiltros.Any(sub => JUGUETES_SUBFILTERS.Contains(sub)))
                        {
                            categoriaPrincipal = "Juguetes";
                        }

                        // 3. Procesar Precio
                        decimal precio = 0.00m;
                        decimal.TryParse(precioStr,
                                         NumberStyles.Currency,
                                         CultureInfo.InvariantCulture,
                                         out precio);

                        // 4. Procesar Imágenes
                        var imagenes = imagenUrlCompleta
                            .Split('|')
                            .Select(url => url.Trim())
                            .Where(url => !string.IsNullOrEmpty(url) && (url.StartsWith("http") || url.StartsWith("https")))
                            .ToList();

                        productos.Add(new Producto
                        {
                            Id = idCounter++,
                            Nombre = nombre,
                            Categoria = categoriaPrincipal, // <-- AHORA DINÁMICO
                            SubFiltros = subFiltros,       // <-- Valores del CSV (para filtros secundarios)
                            Precio = precio,
                            ImagenUrls = imagenes,
                            Descripcion = descripcion
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al cargar productos del CSV: {ex.Message}");
            }

            return productos;
        }
    }
}