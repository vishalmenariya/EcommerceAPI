using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Data
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]{3,100}$", ErrorMessage = "Name can only contains a-zA-Z & space!")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Phone Number is required!")]
        [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters!")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Email is required!")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters!")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Customer Country is required!")]
        [MaxLength(50, ErrorMessage = "Country cannot exceed 50 characters!")]
        [RegularExpression(@"^[a-zA-Z\s]{3,50}$", ErrorMessage = "Country can only contains a-zA-Z & space!")]
        public string CustomerCountry { get; set; }


        [JsonIgnore]
        public ICollection<Order>? Orders { get; set; }
    }
}
