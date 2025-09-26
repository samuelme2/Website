namespace TuProyecto.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public decimal Precio { get; set; }
        public List<string> ImagenUrls { get; set; } = new List<string>();
        public string Descripcion { get; set; }
    }
}
