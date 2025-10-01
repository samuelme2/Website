using Microsoft.AspNetCore.Mvc;
using TuProyecto.Models;
using TuProyecto.Services; // Importa el servicio CSV
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System;

namespace TuProyecto.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private static List<Producto> _productos;

        // Diccionario de equivalencias (subfiltro -> lista de valores que significan lo mismo)
        private static readonly Dictionary<string, List<string>> SubfiltroEquivalencias =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Anales", new List<string> { "Anales", "Estimulación Anal", "Plugs Anales", "Plugs Anales" } }, // <-- ¡ESTO ES LO IMPORTANTE!
                { "Vibradores", new List<string> { "Vibradores" } },
                { "Dildos", new List<string> { "Dildos" } },
                { "Torsos", new List<string> { "Torsos" } },
                { "Sex Machines", new List<string> { "Sex Machines" } }
                // agrega aquí más equivalencias si las necesitas
            };

        public CatalogoController(IWebHostEnvironment env)
        {
            _env = env;

            if (_productos == null)
            {
                // Carga desde CSV (CsvProductoService debe devolver lista con SubFiltros rellenos)
                _productos = CsvProductoService.LoadProductos(_env.WebRootPath) ?? new List<Producto>();

                // Ejemplo: añadir catálogos PDF (opcional)
                _productos.AddRange(new List<Producto>
                {
                    new Producto { Id = 101, Nombre = "Cereza", Categoria = "Lencería", ImagenUrls = new List<string>{ "/images/catalogo1.jpg" } },
                    new Producto { Id = 102, Nombre = "Fantasy", Categoria = "Lencería", ImagenUrls = new List<string>{ "/images/catalogo2.jpg" } }
                });
            }
        }

        public IActionResult Index(string categoria)
        {
            var productosAll = _productos ?? new List<Producto>();

            // Normalizar entrada
            var catRaw = categoria?.Trim();
            if (string.IsNullOrEmpty(catRaw))
            {
                // Sin filtro: devolver todo (o excluir Lencería si lo deseas)
                ViewBag.Categorias = productosAll.Select(p => p.Categoria).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                ViewBag.CategoriaSeleccionada = null;
                return View(productosAll.Where(p => !string.Equals(p.Categoria, "Lencería", StringComparison.OrdinalIgnoreCase)).ToList());
            }

            // Preparar categorías conocidas
            var categoriasPrincipales = productosAll.Select(p => p.Categoria)
                                                   .Where(x => !string.IsNullOrEmpty(x))
                                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                                   .ToList();

            // Si coincide con una categoría principal -> devolver por categoría
            if (categoriasPrincipales.Any(cp => cp.Equals(catRaw, StringComparison.OrdinalIgnoreCase)))
            {
                var byCategoria = productosAll
                    .Where(p => p.Categoria != null && p.Categoria.Equals(catRaw, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                ViewBag.Categorias = categoriasPrincipales;
                ViewBag.CategoriaSeleccionada = catRaw;
                return View(byCategoria);
            }

            // --- Si llegamos aquí, tratamos la petición como SUBFILTRO ---
            // Determinar equivalentes:
            List<string> equivalentes;
            if (SubfiltroEquivalencias.TryGetValue(catRaw, out equivalentes) && equivalentes != null && equivalentes.Count > 0)
            {
                // ya los tenemos
            }
            else
            {
                // si no hay equivalencias definidas, usamos el mismo valor pedido
                equivalentes = new List<string> { catRaw };
            }

            // Filtrar: mostrar solo productos de la categoría "Juguetes" que tengan cualquiera de los subfiltros equivalentes.
            var resultadosSubfiltro = productosAll
                .Where(p =>
                    !string.IsNullOrEmpty(p.Categoria)
                    && p.Categoria.Equals("Juguetes", StringComparison.OrdinalIgnoreCase)
                    && p.SubFiltros != null
                    && p.SubFiltros.Any(sf => equivalentes.Any(eq => string.Equals(eq, sf?.Trim(), StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            ViewBag.Categorias = categoriasPrincipales.Concat(new[] { "Lencería" }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.CategoriaSeleccionada = catRaw;
            return View(resultadosSubfiltro);
        }

        public IActionResult Detalle(int id)
        {
            var producto = _productos?.FirstOrDefault(p => p.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }
    }
}
