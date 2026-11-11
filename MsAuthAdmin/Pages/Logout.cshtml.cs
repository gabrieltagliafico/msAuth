using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        AdminSession.SignOut(HttpContext.Session);
        return RedirectToPage("/Login");
    }
}
