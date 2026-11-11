using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages;

public abstract class AdminPageModel : PageModel
{
    protected AuthApiClient Api { get; }

    protected AdminPageModel(AuthApiClient api)
    {
        Api = api;
    }

    protected string Token
    {
        get
        {
            AdminSession.TryGetToken(HttpContext.Session, out var token);
            return token;
        }
    }

    protected void ShowError(string error)
    {
        ViewData["Error"] = error;
    }

    protected IActionResult? RedirectIfUnauthorized(int status)
    {
        if (status != StatusCodes.Status401Unauthorized)
        {
            return null;
        }

        AdminSession.SignOut(HttpContext.Session);
        return RedirectToPage("/Login");
    }
}
