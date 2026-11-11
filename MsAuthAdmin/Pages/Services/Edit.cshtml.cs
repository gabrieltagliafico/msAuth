using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Services;

public class EditModel : AdminPageModel
{
    public EditModel(AuthApiClient api) : base(api)
    {
    }

    public int IdService { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadAsync(id) ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("El nombre es obligatorio.");
            return await LoadAsync(id) ?? Page();
        }

        var call = await Api.PostAsync<ServiceRow>("api/Services/Update", new
        {
            idService = id,
            name = name.Trim(),
            isActive
        }, Token);

        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync(id) ?? Page();
        }

        TempData["Notice"] = "Servicio actualizado.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult?> LoadAsync(int id)
    {
        var call = await Api.GetAsync<List<ServiceRow>>("api/Services/List", Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            TempData["Error"] = call.Error;
            return RedirectToPage("Index");
        }

        var service = call.Data?.FirstOrDefault(item => item.IdService == id);
        if (service is null)
        {
            TempData["Error"] = "El servicio no está en el listado.";
            return RedirectToPage("Index");
        }

        IdService = service.IdService;
        Name = service.Name ?? string.Empty;
        IsActive = service.IsActive;
        return null;
    }
}
