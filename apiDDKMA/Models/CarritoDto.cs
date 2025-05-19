namespace apiDDKMA.Models
{
    public class CarritoDto
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime Fecha { get; set; }
        public bool Activo { get; set; }
        public decimal Total { get; set; }
        public List<CarritoItemDto> Items { get; set; } = new List<CarritoItemDto>();
        public List<CarritoCustomItemDto> CustomItems { get; set; } = new List<CarritoCustomItemDto>();
    }
}
