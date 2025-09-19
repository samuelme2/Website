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
            new Producto { Id = 1, Nombre = "Lubricante", Categoria = "Lubricantes", Precio = 20000, ImagenUrl = "/images/lubricante.jpg", Descripcion = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis interdum erat eros, in sollicitudin libero condimentum non. Phasellus pretium nisi purus, id feugiat urna congue quis. Nam augue neque, consectetur eget feugiat eu, mattis id diam. In aliquam consectetur ullamcorper. Mauris eleifend magna ac ante sollicitudin, in congue orci feugiat. Maecenas elit arcu, ullamcorper sed arcu vel, mattis laoreet nibh. Vivamus ullamcorper, nunc quis suscipit ultrices, eros diam luctus eros, sit amet vulputate nisi quam a metus. Nullam elementum porta libero ac fringilla. Aliquam id velit et urna pretium dignissim. Mauris tristique dolor vitae nisl rutrum, semper semper sapien mattis. Etiam cursus justo eu sollicitudin lacinia. "},
            new Producto { Id = 2, Nombre = "Vibrador", Categoria = "Vibradores", Precio = 80000, ImagenUrl = "/images/vibrador.jpg", Descripcion = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis interdum erat eros, in sollicitudin libero condimentum non. Phasellus pretium nisi purus, id feugiat urna congue quis. Nam augue neque, consectetur eget feugiat eu, mattis id diam. In aliquam consectetur ullamcorper. Mauris eleifend magna ac ante sollicitudin, in congue orci feugiat. Maecenas elit arcu, ullamcorper sed arcu vel, mattis laoreet nibh. Vivamus ullamcorper, nunc quis suscipit ultrices, eros diam luctus eros, sit amet vulputate nisi quam a metus. Nullam elementum porta libero ac fringilla. Aliquam id velit et urna pretium dignissim. Mauris tristique dolor vitae nisl rutrum, semper semper sapien mattis. Etiam cursus justo eu sollicitudin lacinia." },
            new Producto { Id = 3, Nombre = "Preservativos", Categoria = "Preservativos", Precio = 15000, ImagenUrl = "/images/preservativo.jpg", Descripcion = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis interdum erat eros, in sollicitudin libero condimentum non. Phasellus pretium nisi purus, id feugiat urna congue quis. Nam augue neque, consectetur eget feugiat eu, mattis id diam. In aliquam consectetur ullamcorper. Mauris eleifend magna ac ante sollicitudin, in congue orci feugiat. Maecenas elit arcu, ullamcorper sed arcu vel, mattis laoreet nibh. Vivamus ullamcorper, nunc quis suscipit ultrices, eros diam luctus eros, sit amet vulputate nisi quam a metus. Nullam elementum porta libero ac fringilla. Aliquam id velit et urna pretium dignissim. Mauris tristique dolor vitae nisl rutrum, semper semper sapien mattis. Etiam cursus justo eu sollicitudin lacinia." },
            new Producto { Id = 4, Nombre = "Satisfyer", Categoria = "Vibradores", Precio = 300000, ImagenUrl = "/images/Satisfayer.jpg", Descripcion = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis interdum erat eros, in sollicitudin libero condimentum non. Phasellus pretium nisi purus, id feugiat urna congue quis. Nam augue neque, consectetur eget feugiat eu, mattis id diam. In aliquam consectetur ullamcorper. Mauris eleifend magna ac ante sollicitudin, in congue orci feugiat. Maecenas elit arcu, ullamcorper sed arcu vel, mattis laoreet nibh. Vivamus ullamcorper, nunc quis suscipit ultrices, eros diam luctus eros, sit amet vulputate nisi quam a metus. Nullam elementum porta libero ac fringilla. Aliquam id velit et urna pretium dignissim. Mauris tristique dolor vitae nisl rutrum, semper semper sapien mattis. Etiam cursus justo eu sollicitudin lacinia." },
            new Producto { Id = 5, Nombre = "Plug", Categoria = "Plug", Precio = 5000, ImagenUrl = "/images/plug.jpg", Descripcion = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis interdum erat eros, in sollicitudin libero condimentum non. Phasellus pretium nisi purus, id feugiat urna congue quis. Nam augue neque, consectetur eget feugiat eu, mattis id diam. In aliquam consectetur ullamcorper. Mauris eleifend magna ac ante sollicitudin, in congue orci feugiat. Maecenas elit arcu, ullamcorper sed arcu vel, mattis laoreet nibh. Vivamus ullamcorper, nunc quis suscipit ultrices, eros diam luctus eros, sit amet vulputate nisi quam a metus. Nullam elementum porta libero ac fringilla. Aliquam id velit et urna pretium dignissim. Mauris tristique dolor vitae nisl rutrum, semper semper sapien mattis. Etiam cursus justo eu sollicitudin lacinia." }

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
