using System.ComponentModel.DataAnnotations;

namespace apiDDKMA.Models
{
    public class AddCustomToCartRequest
    {
        [Required]
        public int CustomProductId { get; set; }

        [Required, Range(1, 100)]
        public int Quantity { get; set; }

        [Required, Range(0, 10000)]
        public decimal UnitPrice { get; set; }
    }
}
