﻿namespace TuProyecto.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public string Descripcion { get; set; }
    }
}
