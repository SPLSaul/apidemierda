namespace apiDDKMA.Models
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int FkCarrito { get; set; }
        public int FkPastel { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
