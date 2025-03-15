using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages;

public class ContactsModel : PageModel
{
    private readonly ILogger<ContactsModel> _logger;

    public ContactsModel(ILogger<ContactsModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}