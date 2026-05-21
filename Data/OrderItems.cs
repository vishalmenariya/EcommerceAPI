using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Data
{
    public class OrderItems
    {
        public int Id { get; set; }

        [Range(1, 1000, ErrorMessage = "You can only order between 1 and 1000 items.")]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "1.00", "100000.00", ErrorMessage = "Price must be between 1.00 and 1,00,000")]
        public Decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}