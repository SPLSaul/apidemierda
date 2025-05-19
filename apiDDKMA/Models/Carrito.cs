namespace apiDDKMA.Models
{
    public class Carrito
    {
        public int Id { get; set; }
        public int FkUsuario { get; set; }  // Clave foránea
        public DateTime Fecha { get; set; }
        public bool Activo { get; set; }
    }
}
