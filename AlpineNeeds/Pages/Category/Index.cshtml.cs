using AlpineNeeds.Data;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Category;

public class CategoryIndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IEnumerable<AlpineNeeds.Models.Category> Categories { get; set; }

    public CategoryIndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public void OnGet()
    {
        Categories = _db.Categories.ToList();
    }
}