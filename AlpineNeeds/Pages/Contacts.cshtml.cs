using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages;

public class ContactsModel(ILogger<ContactsModel> logger) : PageModel
{
    public void OnGet()
    {
    }
}