using System.ComponentModel.DataAnnotations;

namespace AlpineNeeds.Models
{
    public class CheckoutAddressViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Address Line 1")]
        public string AddressLine1 { get; set; } = string.Empty;

        [Display(Name = "Address Line 2")]
        public string? AddressLine2 { get; set; }

        [Required]
        public string City { get; set; } = string.Empty;

        [Display(Name = "State/Province")]
        public string? State { get; set; } = string.Empty;

        [Required]
        [Display(Name = "ZIP/Postal Code")]
        [RegularExpression(@"^[0-9A-Za-z\- ]+$", ErrorMessage = "Invalid ZIP/Postal code.")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public string Country { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Address Type")]
        public string AddressType { get; set; } = "Home";

        [Display(Name = "Save address for future orders")]
        public bool SaveForFuture { get; set; }
    }
}
