using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        public required string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        // Shipping Address
        public int? ShippingAddressId { get; set; }
        
        [ForeignKey("ShippingAddressId")]
        public virtual Address? ShippingAddress { get; set; }
        
        // Billing Address
        public int? BillingAddressId { get; set; }
        
        [ForeignKey("BillingAddressId")]
        public virtual Address? BillingAddress { get; set; }
        
        public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
    
    public enum OrderStatus
    {
        [Display(Name = "Placed")]
        Placed,
        [Display(Name = "Confirmed")]
        Confirmed,
        [Display(Name = "Preparing")]
        Preparing,
        [Display(Name = "Packed")]
        Packed,
        [Display(Name = "Delivered")]
        Delivered,
        [Display(Name = "Finished")]
        Finished,
        [Display(Name = "Customer Canceled")]
        CustomerCanceled,
        [Display(Name = "Out Of Stock")]
        OutOfStock
    }
}
