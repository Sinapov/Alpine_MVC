using AlpineNeeds.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Category;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    [BindProperty]
    public AlpineNeeds.Models.Category Category { get; set; }

    public CreateModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            await _db.Categories.AddAsync(Category);
            await _db.SaveChangesAsync();
            return RedirectToPage("Index");
        }
        return Page();
    }
}