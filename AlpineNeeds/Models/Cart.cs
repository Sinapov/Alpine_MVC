using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }
        
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
