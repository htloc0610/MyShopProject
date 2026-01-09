using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyShopAPI.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int ProductId { get; set; }

        [JsonIgnore] // Prevent cycle
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;
    }
}
