using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Data
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime OrderDate { get; set; }

        public int CustomerId { get; set; }

        public Customer? Customer { get; set; }


        [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public ICollection<OrderItems> Items { get; set; }
    }
}