using Microsoft.AspNetCore.Mvc;
using TuProyecto.Models;
using System.Collections.Generic;
using System.Linq;

namespace TuProyecto.Controllers
{
    public class CatalogoController : Controller
    {
        private static List<Producto> _productos = new List<Producto>
        {
            new Producto { Id = 1, Nombre = "Lubricante", Categoria = "Lubricantes", Precio = 20000, ImagenUrl = "/images/lubricante.jpg" },
            new Producto { Id = 2, Nombre = "Vibrador", Categoria = "Vibradores", Precio = 80000, ImagenUrl = "/images/vibrador.jpg" },
            new Producto { Id = 3, Nombre = "Preservativos", Categoria = "Preservativos", Precio = 15000, ImagenUrl = "/images/preservativo.jpg" },
            new Producto { Id = 4, Nombre = "Satisfyer", Categoria = "Vibradores", Precio = 300000, ImagenUrl = "/images/Satisfayer.jpg" },
            new Producto { Id = 5, Nombre = "Plug", Categoria = "Plug", Precio = 5000, ImagenUrl = "/images/plug.jpg" }
        };

        public IActionResult Index(string categoria)
        {
            // 🔥 Si es "Lencería", mostrar SOLO catálogos PDF
            if (!string.IsNullOrEmpty(categoria) && categoria == "Lencería")
            {
                var catalogos = new List<Producto>
                {
                    new Producto { Id = 101, Nombre = "Cereza", Categoria = "Catalogos", ImagenUrl = "/images/catalogo1.jpg" },
                    new Producto { Id = 102, Nombre = "Fantasy", Categoria = "Catalogos", ImagenUrl = "/images/catalogo2.jpg" }
                };

                ViewBag.EsCatalogoPdf = true;
                return View(catalogos);
            }

            // 🔥 En cualquier otra categoría mostramos productos normales
            ViewBag.EsCatalogoPdf = false;

            var productos = string.IsNullOrEmpty(categoria)
                ? _productos
                : _productos.Where(p => p.Categoria == categoria).ToList();

            ViewBag.Categorias = _productos.Select(p => p.Categoria).Distinct().ToList();
            ViewBag.CategoriaSeleccionada = categoria;

            return View(productos);
        }

        public IActionResult Productos()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Detalle(int id)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null) return NotFound();

            return View(producto);
        }
    }
}
