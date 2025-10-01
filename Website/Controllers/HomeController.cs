using Microsoft.AspNetCore.Mvc;
using TuProyecto.Models;
using TuProyecto.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _env;
    private static List<Producto> _productos;

    public HomeController(IWebHostEnvironment env)
    {
        _env = env;

        if (_productos == null)
        {
            _productos = CsvProductoService.LoadProductos(_env.WebRootPath) ?? new List<Producto>();
        }
    }

    public IActionResult Index()
    {
        // Tomar los productos más recientes o destacados
        var productosDestacados = _productos.OrderByDescending(p => p.Id).Take(6).ToList();

        ViewBag.ProductosDestacados = productosDestacados;
        return View();
    }
}
