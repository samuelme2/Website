using Microsoft.AspNetCore.Mvc;
using TuProyecto.Models;
using TuProyecto.Services; // 👈 Importar el nuevo servicio
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting; // 👈 Para inyectar la ruta del proyecto

namespace TuProyecto.Controllers
{
    public class CatalogoController : Controller
    {
        // Variable privada para el entorno de hosting (para la ruta del archivo)
        private readonly IWebHostEnvironment _env;

        // Lista estática para almacenar los productos cargados
        private static List<Producto> _productos;

        // Constructor para inyectar IWebHostEnvironment
        public CatalogoController(IWebHostEnvironment env)
        {
            _env = env;

            // Inicializar la lista de productos SÓLO si está vacía.
            // Esto asegura que la lectura del CSV se haga solo una vez por ejecución del programa.
            if (_productos == null)
            {
                // 1. Cargar los productos del CSV (Scrapeados)
                _productos = CsvProductoService.LoadProductos(_env.WebRootPath);

                // 2. Agregar los catálogos PDF fijos (opcional, si los quieres en la misma lista)
                // Aunque los manejas aparte, es bueno tenerlos en la lista si usas ViewBag.Categorias
                _productos.AddRange(new List<Producto>
                {new Producto { Id = 101, Nombre = "Cereza", Categoria = "Lencería", ImagenUrls = new List<string> { "/images/catalogo1.jpg" } },
                new Producto { Id = 102, Nombre = "Fantasy", Categoria = "Lencería", ImagenUrls = new List<string> { "/images/catalogo2.jpg" } }

                });

                // Si quieres añadir un producto estático de ejemplo:
                // _productos.Add(new Producto { Id = 99, Nombre = "Ejemplo Fijo", Categoria = "Estaticos", Precio = 5000, ImagenUrl = "/images/ejemplo.jpg" });

            }
        }

        public IActionResult Index(string categoria)
        {
            // La lógica para "Lencería" la dejamos igual, aunque ahora los catálogos
            // ya están en la lista _productos para que aparezcan en ViewBag.Categorias.
            if (!string.IsNullOrEmpty(categoria) && categoria == "Lencería")
            {
                var catalogos = _productos.Where(p => p.Categoria == "Lencería").ToList();
                ViewBag.EsCatalogoPdf = true;
                return View(catalogos);
            }

            // ... (Resto de la lógica es la misma)

            ViewBag.EsCatalogoPdf = false;

            var productos = string.IsNullOrEmpty(categoria)
                ? _productos.Where(p => p.Categoria != "Lencería").ToList() // Excluir lencería por defecto
                : _productos.Where(p => p.Categoria == categoria).ToList();

            // Asegurar que las categorías incluyen "Lencería" si quieres que el filtro funcione en el Layout
            var categoriasDisponibles = _productos.Select(p => p.Categoria).Distinct().ToList();
            if (!categoriasDisponibles.Contains("Lencería"))
            {
                categoriasDisponibles.Add("Lencería");
            }

            ViewBag.Categorias = categoriasDisponibles;
            ViewBag.CategoriaSeleccionada = categoria;

            return View(productos);
        }

        // Resto de métodos (Productos, Privacy, Contact, Detalle) sin cambios.
        // Asegúrate de que el método Detalle pueda encontrar los nuevos IDs:

        public IActionResult Detalle(int id)
        {
            // El ID es dinámico ahora, así que la búsqueda es la clave.
            var producto = _productos.FirstOrDefault(p => p.Id == id);
            if (producto == null) return NotFound();

            return View(producto);
        }
    }
}