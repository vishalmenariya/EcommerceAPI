using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Data
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required!")]
        [MinLength(3, ErrorMessage = "Product Name length Must be greater than or equal to 3!")]
        [MaxLength(100,ErrorMessage = "Product Name length can't exceed 100!")]
        [RegularExpression(@"^[a-zA-Z\s]{3,50}$", ErrorMessage = "Product Name can only contains a-zA-Z & space!")]
        public string ProductName { get; set; }

        [Range(0, 1000, ErrorMessage = "Stock must be between 0 and 1,000!")]
        public int ProductStock { get; set; } = 0;

        [Range(typeof(decimal), "1.00", "100000.00", ErrorMessage = "Price must be between 0 to 1,00,000!")]
        public Decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "Product Category is required!")]
        [MinLength(3, ErrorMessage = "Product Category length Must be greater than or equal to 3!")]
        [MaxLength(50, ErrorMessage = "Product Category length can't exceed 100!")]
        [RegularExpression(@"^[a-zA-Z\s]{3,50}$", ErrorMessage = "Produt Category can only contains a-zA-Z & space!")]
        public string ProductCategory { get; set; }

        [JsonIgnore]
        public ICollection<OrderItems>? Items { get; set; }
    }
}