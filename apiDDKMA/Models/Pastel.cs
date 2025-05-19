using System;
using System.ComponentModel.DataAnnotations;

namespace apiDDKMA.Models
{
    public class Pastel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(500)]
        public string Descripcion { get; set; }

        [Required, Range(0, 1000)]
        public decimal Precio { get; set; }

        public string Imagen { get; set; }

        public bool Destacado { get; set; }

        [Range(0, 1000)]
        public int Stock { get; set; }

        public bool Disponible { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public bool Deleted { get; set; } = false;
    }
}