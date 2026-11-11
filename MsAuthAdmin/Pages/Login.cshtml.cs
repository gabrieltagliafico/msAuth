using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages;

public class LoginModel : PageModel
{
    private readonly AuthApiClient _api;
    private readonly AuthApiOptions _options;

    public LoginModel(AuthApiClient api, IOptions<AuthApiOptions> options)
    {
        _api = api;
        _options = options.Value;
    }

    [BindProperty]
    [Required(ErrorMessage = "El GUID del usuario es obligatorio.")]
    public string UidUser { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "La API key es obligatoria.")]
    public string ApiKey { get; set; } = string.Empty;

    [BindProperty]
    [Range(1, int.MaxValue, ErrorMessage = "El id del servicio debe ser un entero positivo.")]
    public int IdService { get; set; }

    public string? ApiHost { get; private set; }

    public IActionResult OnGet()
    {
        ApiHost = ReadHost();
        if (IdService == 0)
        {
            IdService = _options.ServiceId;
        }

        if (AdminSession.IsActive(HttpContext.Session))
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ApiHost = ReadHost();
        if (!Guid.TryParse(UidUser, out var uidUser) || uidUser == Guid.Empty)
        {
            ModelState.AddModelError(nameof(UidUser), "El GUID del usuario no es válido.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Error"] = string.Join(" ", ModelState.Values.SelectMany(entry => entry.Errors).Select(error => error.ErrorMessage));
            return Page();
        }

        var call = await _api.IssueTokenAsync(uidUser, ApiKey, IdService);
        if (!call.Ok || string.IsNullOrWhiteSpace(call.Data?.Token))
        {
            ViewData["Error"] = string.IsNullOrWhiteSpace(call.Error)
                ? "No se pudo emitir el token."
                : call.Error;
            return Page();
        }

        AdminSession.SignIn(HttpContext.Session, call.Data.Token, call.Data.DtExpires, uidUser);
        return RedirectToPage("/Index");
    }

    private string? ReadHost()
    {
        return Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var uri)
            ? uri.GetLeftPart(UriPartial.Authority)
            : null;
    }
}
