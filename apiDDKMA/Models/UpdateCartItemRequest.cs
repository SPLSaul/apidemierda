using System.ComponentModel.DataAnnotations;

namespace apiDDKMA.Models
{
    public class UpdateCartItemRequest
    {
        [Required]
        [Range(1, 100)]
        public int NewQuantity { get; set; }
    }
}
