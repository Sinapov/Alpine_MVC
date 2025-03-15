using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AlpineNeeds.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        [PersonalData]
        public string? FirstName { get; set; }
        
        [StringLength(50)]
        [PersonalData]
        public string? LastName { get; set; }
        
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}