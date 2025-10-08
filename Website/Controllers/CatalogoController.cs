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
                { "Anales", new List<string> { "Anales", "Estimulación Anal", "Plugs Anales" } },
                { "Vibradores", new List<string> { "Vibradores" } },
                { "Dildos", new List<string> { "Dildos" } },
                { "Torsos", new List<string> { "Torsos" } },
                { "Sex Machines", new List<string> { "Sex Machines" } }
            };

        public CatalogoController(IWebHostEnvironment env)
        {
            _env = env;

            if (_productos == null)
            {
                // Carga desde CSV (robusta)
                _productos = CsvProductoService.LoadProductos(_env.WebRootPath) ?? new List<Producto>();

                // Ejemplo: añadir catálogos PDF (opcional)
                _productos.AddRange(new List<Producto>
                {
                    new Producto { Id = 101, Nombre = "Cereza", Categoria = "Lencería", ImagenUrls = new List<string>{ "/images/catalogo1.jpg" } },
                    new Producto { Id = 102, Nombre = "Fantasy", Categoria = "Lencería", ImagenUrls = new List<string>{ "/images/catalogo2.jpg" } }
                });
            }
        }

        // Normaliza/limpia un valor (quita espacios y barras verticales sobrantes)
        private static string Normalize(string s)
        {
            return s == null ? null : s.Trim().Trim('|', ' ', '\t', '\r', '\n');
        }

        // Separa partes de categoria cuando vienen con pipe: "Lovense|Accesorios"
        private static IEnumerable<string> SplitCategoryParts(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria)) yield break;
            foreach (var part in categoria.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                var n = Normalize(part);
                if (!string.IsNullOrEmpty(n)) yield return n;
            }
        }

        public IActionResult Index(string categoria)
        {
            var productosAll = _productos ?? new List<Producto>();
            var catRaw = categoria?.Trim();

            // Sin filtro: devolver todo (o excluir Lencería si lo deseas)
            if (string.IsNullOrEmpty(catRaw))
            {
                var categoriasPrincipales = productosAll
                    .SelectMany(p => SplitCategoryParts(p.Categoria))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                ViewBag.Categorias = categoriasPrincipales;
                ViewBag.CategoriaSeleccionada = null;
                return View(productosAll
                    .Where(p => !string.Equals(Normalize(p.Categoria), "Lencería", StringComparison.OrdinalIgnoreCase)
                                && !SplitCategoryParts(p.Categoria).Any(cp => cp.Equals("Lencería", StringComparison.OrdinalIgnoreCase)))
                    .ToList());
            }

            var catNormalized = Normalize(catRaw);

            // Preparar categorías principales a partir de las partes (maneja "Lovense|Accesorios")
            var categoriasPrincipalesList = productosAll
                .SelectMany(p => SplitCategoryParts(p.Categoria))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Si coincide con una categoría principal -> devolver por esa categoría
            if (categoriasPrincipalesList.Any(cp => cp.Equals(catNormalized, StringComparison.OrdinalIgnoreCase)))
            {
                var byCategoria = productosAll
                    .Where(p =>
                        SplitCategoryParts(p.Categoria).Any(part => part.Equals(catNormalized, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                ViewBag.Categorias = categoriasPrincipalesList;
                ViewBag.CategoriaSeleccionada = catRaw;
                return View(byCategoria);
            }

            // --- Si llegamos aquí, tratamos la petición como SUBFILTRO (ej: Dildos, Vibradores, etc.) ---
            List<string> equivalentes;
            if (!SubfiltroEquivalencias.TryGetValue(catRaw, out equivalentes) || equivalentes == null || equivalentes.Count == 0)
            {
                equivalentes = new List<string> { catRaw };
            }

            // Filtrar productos que pertenecen a la categoría Juguetes y que tengan subfiltros equivalentes
            var resultadosSubfiltro = productosAll
                .Where(p =>
                    SplitCategoryParts(p.Categoria).Any(part => part.Equals("Juguetes", StringComparison.OrdinalIgnoreCase))
                    && p.SubFiltros != null
                    && p.SubFiltros.Any(sf => equivalentes.Any(eq => string.Equals(Normalize(eq), Normalize(sf), StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            if (resultadosSubfiltro.Any())
            {
                ViewBag.Categorias = categoriasPrincipalesList.Concat(new[] { "Lencería" }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                ViewBag.CategoriaSeleccionada = catRaw;
                return View(resultadosSubfiltro);
            }

            // --- Fallback: buscar coincidencias en SubFiltros O en partes de Categoria ---
            var fallbackResults = productosAll
                .Where(p =>
                    SplitCategoryParts(p.Categoria).Any(part => part.Equals(catNormalized, StringComparison.OrdinalIgnoreCase))
                    || (p.SubFiltros != null && p.SubFiltros.Any(sf => string.Equals(Normalize(sf), catNormalized, StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            ViewBag.Categorias = categoriasPrincipalesList.Concat(new[] { "Lencería" }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.CategoriaSeleccionada = catRaw;
            return View(fallbackResults);
        }

        public IActionResult Detalle(int id)
        {
            var producto = _productos?.FirstOrDefault(p => p.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }
    }
}
