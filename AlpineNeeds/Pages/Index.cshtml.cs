using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages;

public class IndexModel(ILogger<IndexModel> logger) : PageModel
{
    public void OnGet()
    {
    }
}