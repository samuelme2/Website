using Microsoft.AspNetCore.Mvc;
using TuProyecto.Models;
using TuProyecto.Services;
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
                { "Sex Machines", new List<string> { "Sex Machines", "Sex Machine" } } // añadí "Sex Machine" por equivalencia
            };

        public CatalogoController(IWebHostEnvironment env)
        {
            _env = env;

            if (_productos == null)
            {
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
            var catNormalized = Normalize(catRaw);

            // Sin filtro: devolver todo salvo Lencería (igual que antes)
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
                    .Where(p => !SplitCategoryParts(p.Categoria)
                        .Any(cp => cp.Equals("Lencería", StringComparison.OrdinalIgnoreCase)))
                    .ToList());
            }

            // Lista general de categorías principales (para la UI)
            var categoriasPrincipalesList = productosAll
                .SelectMany(p => SplitCategoryParts(p.Categoria))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // --- CASO ESPECIAL: "Juguetes" debe devolver TODOS los productos relacionados a juguetes ---
            if (!string.IsNullOrEmpty(catNormalized) && catNormalized.Equals("Juguetes", StringComparison.OrdinalIgnoreCase))
            {
                // Construimos la lista de subcategorías que consideraremos "pertenecientes a Juguetes"
                var jugueteSubcats = SubfiltroEquivalencias
                    .SelectMany(kv => kv.Value)
                    .Select(x => Normalize(x))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // Añadimos además las claves del diccionario (por si se usaron esos nombres directamente)
                jugueteSubcats.AddRange(SubfiltroEquivalencias.Keys.Select(k => Normalize(k)));
                jugueteSubcats = jugueteSubcats
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var juguetes = productosAll
                    .Where(p =>
                        // 1) productos que explícitamente tienen "Juguetes" en su campo Categoria (ej: "Juguetes|Sex Machine")
                        SplitCategoryParts(p.Categoria).Any(part => string.Equals(Normalize(part), "Juguetes", StringComparison.OrdinalIgnoreCase))
                        // 2) O productos cuya categoría (una de las partes) sea alguna subcategoría de juguete (ej: "Sex Machine", "Dildos")
                        || SplitCategoryParts(p.Categoria).Any(part => jugueteSubcats.Any(s => string.Equals(Normalize(part), s, StringComparison.OrdinalIgnoreCase)))
                        // 3) O productos cuyos SubFiltros contengan alguna de esas subcategorías/equivalencias
                        || (p.SubFiltros != null && p.SubFiltros.Any(sf => jugueteSubcats.Any(s => string.Equals(Normalize(sf), s, StringComparison.OrdinalIgnoreCase))))
                    )
                    .Distinct()
                    .ToList();

                ViewBag.Categorias = categoriasPrincipalesList.Concat(new[] { "Lencería" })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                ViewBag.CategoriaSeleccionada = "Juguetes";
                return View(juguetes);
            }

            // --- Si coincide con una categoría principal normal (ej: Lovense, Lubricantes, etc.)
            if (categoriasPrincipalesList.Any(cp => cp.Equals(catNormalized, StringComparison.OrdinalIgnoreCase)))
            {
                var byCategoria = productosAll
                    .Where(p => SplitCategoryParts(p.Categoria)
                        .Any(part => part.Equals(catNormalized, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                ViewBag.Categorias = categoriasPrincipalesList;
                ViewBag.CategoriaSeleccionada = catRaw;
                return View(byCategoria);
            }

            // --- Tratar como subfiltro (ej: Dildos, Vibradores, etc.)
            if (!SubfiltroEquivalencias.TryGetValue(catRaw, out var equivalentes) || equivalentes == null)
            {
                equivalentes = new List<string> { catRaw };
            }

            // Expandir equivalentes normalizados
            var equivalentesNormalized = equivalentes.Select(e => Normalize(e)).Where(e => !string.IsNullOrEmpty(e)).ToList();

            var resultadosSubfiltro = productosAll
                .Where(p =>
                    // debe pertenecer a la familia "Juguetes" (si tu data usa "Juguetes|X")
                    SplitCategoryParts(p.Categoria).Any(part => part.Equals("Juguetes", StringComparison.OrdinalIgnoreCase))
                    && p.SubFiltros != null
                    && p.SubFiltros.Any(sf => equivalentesNormalized.Any(eq => string.Equals(Normalize(sf), eq, StringComparison.OrdinalIgnoreCase)))
                )
                .ToList();

            if (resultadosSubfiltro.Any())
            {
                ViewBag.Categorias = categoriasPrincipalesList.Concat(new[] { "Lencería" }).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                ViewBag.CategoriaSeleccionada = catRaw;
                return View(resultadosSubfiltro);
            }

            // Fallback final: buscar coincidencias en partes de Categoria o SubFiltros
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
