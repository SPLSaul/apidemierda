namespace apiDDKMA.Models
{
    public class CarritoCustomItemDto
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int PersonalizadoId { get; set; }
        public string NombrePersonalizado { get; set; }
        public string ImagenPersonalizado { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}
