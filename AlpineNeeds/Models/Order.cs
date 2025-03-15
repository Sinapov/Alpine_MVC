using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AlpineNeeds.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
        
        public OrderStatus Status { get; set; }
        
        public DateTime OrderDate { get; set; }
        
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }
    }
    
    public enum OrderStatus
    {
        Placed,
        Confirmed,
        Preparing,
        Packed,
        Delivered,
        Finished,
        CustomerCanceled,
        OutOfStock
    }
}
