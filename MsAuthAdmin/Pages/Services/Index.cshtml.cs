using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Services;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public IReadOnlyList<ServiceRow> Services { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        return await LoadAsync() ?? Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("El nombre es obligatorio.");
            return await LoadAsync() ?? Page();
        }

        var call = await Api.PostAsync<ServiceRow>("api/Services/Create", new { name = name.Trim() }, Token);
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

        TempData["Notice"] = "Servicio creado.";
        return RedirectToPage();
    }

    private async Task<IActionResult?> LoadAsync()
    {
        var call = await Api.GetAsync<List<ServiceRow>>("api/Services/List", Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            Services = [];
            return null;
        }

        Services = call.Data ?? [];
        return null;
    }
}
