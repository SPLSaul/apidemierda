using System.ComponentModel.DataAnnotations;

namespace apiDDKMA.Models
{
    // AddToCartRequest.cs
    public class AddToCartRequest
    {
        [Required]
        public int UserId { get; set; }  // Nuevo campo requerido

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Quantity { get; set; }
    }
}
