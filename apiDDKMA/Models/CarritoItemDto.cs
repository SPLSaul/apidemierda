namespace apiDDKMA.Models
{
    public class CarritoItemDto
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }  // Cambiado de FkCarrito
        public int PastelId { get; set; }
        public string NombrePastel { get; set; }
        public string ImagenPastel { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

}
