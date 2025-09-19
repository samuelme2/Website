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
            new Producto { Id = 1, Nombre = "Lubricante", Categoria = "Lubricantes", Precio = 20000, ImagenUrl = "/images/lubricante.jpg", Descripcion = "Lubricante a base de agua." },
            new Producto { Id = 2, Nombre = "Vibrador", Categoria = "Vibradores", Precio = 80000, ImagenUrl = "/images/vibrador.jpg", Descripcion = "Vibrador clásico." },
            new Producto { Id = 3, Nombre = "Preservativos", Categoria = "Preservativos", Precio = 15000, ImagenUrl = "/images/preservativo.jpg", Descripcion = "Caja de 12 preservativos." }
        };

        public IActionResult Index(string categoria)
        {
            var productos = string.IsNullOrEmpty(categoria)
                ? _productos
                : _productos.Where(p => p.Categoria == categoria).ToList();

            ViewBag.Categorias = _productos.Select(p => p.Categoria).Distinct().ToList();
            ViewBag.CategoriaSeleccionada = categoria;

            return View(productos);
        }

        public IActionResult Detalle(int id)
        {
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null) return NotFound();
            return View(producto);
        }
    }
}
