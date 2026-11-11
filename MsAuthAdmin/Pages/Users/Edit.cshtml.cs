using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Users;

public class EditModel : AdminPageModel
{
    public EditModel(AuthApiClient api) : base(api)
    {
    }

    public int IdUser { get; private set; }
    public Guid UidUser { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var loaded = await LoadAsync(id);
        if (loaded is not null)
        {
            return loaded;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string name, bool isActive, string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("El nombre es obligatorio.");
            var reloaded = await LoadAsync(id);
            return reloaded ?? Page();
        }

        var call = await Api.PostAsync<UserRow>("api/Users/Update", new
        {
            idUser = id,
            name = name.Trim(),
            isActive,
            apiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey
        }, Token);

        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            var reloaded = await LoadAsync(id);
            return reloaded ?? Page();
        }

        TempData["Notice"] = "Usuario actualizado.";
        return RedirectToPage("Index");
    }

    private async Task<IActionResult?> LoadAsync(int id)
    {
        var call = await Api.GetAsync<List<UserRow>>("api/Users/List", Token);
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

        var user = call.Data?.FirstOrDefault(item => item.IdUser == id);
        if (user is null)
        {
            TempData["Error"] = "El usuario no está en el listado.";
            return RedirectToPage("Index");
        }

        IdUser = user.IdUser;
        UidUser = user.UidUser;
        Name = user.Name ?? string.Empty;
        IsActive = user.IsActive;
        return null;
    }
}
