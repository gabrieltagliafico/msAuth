using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Users;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public IReadOnlyList<UserRow> Users { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(apiKey))
        {
            ShowError("El nombre y la API key son obligatorios.");
            return await LoadAsync() ?? Page();
        }

        var call = await Api.PostAsync<UserRow>("api/Users/Create", new { name = name.Trim(), apiKey }, Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync() ?? Page();
        }

        TempData["Notice"] = "Usuario creado. El GUID nuevo está en el listado.";
        return RedirectToPage();
    }

    private async Task<IActionResult?> LoadAsync()
    {
        var call = await Api.GetAsync<List<UserRow>>("api/Users/List", Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            Users = [];
            return null;
        }

        Users = call.Data ?? [];
        return null;
    }
}
