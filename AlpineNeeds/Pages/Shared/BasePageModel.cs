using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlpineNeeds.Pages.Shared
{
    public abstract class BasePageModel : PageModel
    {
        protected void AddError(string message, string? key = null)
        {
            ModelState.AddModelError(key ?? string.Empty, message);
        }

        protected void AddPageError(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void AddPageSuccess(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected bool HasModelErrors => !ModelState.IsValid;
    }
}