using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlpineNeeds.Models
{
  public class Category
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public int DisplayOrder { get; set; }
    
    public int? ParentCategoryId { get; set; }
    
    [ForeignKey("ParentCategoryId")]
    public virtual Category? ParentCategory { get; set; }
    
    // Added ImageUrl property for category images
    public string? ImageUrl { get; set; }
  }
}
