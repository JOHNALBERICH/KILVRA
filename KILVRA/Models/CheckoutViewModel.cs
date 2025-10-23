using System.ComponentModel.DataAnnotations;

namespace KILVRA.Models
{
    public class CheckoutViewModel
    {
        // Billing info
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Mobile { get; set; }
        [Required] public string Address1 { get; set; }
        public string Address2 { get; set; }
        [Required] public string Country { get; set; }
        [Required] public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        // Shipping option (optional)
        public bool ShipToDifferentAddress { get; set; }

        // Payment method
        [Required] public string PaymentMethod { get; set; }

        // Cart items
        public IEnumerable<Product> CartItems { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }

    }
}
