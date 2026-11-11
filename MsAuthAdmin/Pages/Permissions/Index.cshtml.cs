using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Permissions;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public IReadOnlyList<ServiceRow> Services { get; private set; } = [];
    public IReadOnlyList<EndpointRow> Endpoints { get; private set; } = [];
    public IReadOnlyList<PermissionRow> Permissions { get; private set; } = [];
    public int IdUser { get; private set; }
    public int IdService { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? idUser, int? idService)
    {
        return await LoadAsync(idUser, idService) ?? Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int idUser, int idService, int idEndpoint)
    {
        var call = await Api.PostAsync<PermissionRow>("api/Permissions/Create", new { idUser, idService, idEndpoint }, Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync(idUser, idService) ?? Page();
        }

        TempData["Notice"] = "Permiso autorizado.";
        return RedirectToPage(new { idUser, idService });
    }

    public async Task<IActionResult> OnPostUpdateAsync(int idPermission, bool isActive, int idUser, int idService)
    {
        var call = await Api.PostAsync<PermissionRow>("api/Permissions/Update", new { idPermission, isActive }, Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync(idUser, idService) ?? Page();
        }

        TempData["Notice"] = "Permiso actualizado.";
        return RedirectToPage(new { idUser, idService });
    }

    private async Task<IActionResult?> LoadAsync(int? idUser, int? idService)
    {
        var users = await Api.GetAsync<List<UserRow>>("api/Users/List", Token);
        var denied = RedirectIfUnauthorized(users.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!users.Ok)
        {
            ShowError(users.Error);
            return null;
        }

        var services = await Api.GetAsync<List<ServiceRow>>("api/Services/List", Token);
        denied = RedirectIfUnauthorized(services.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!services.Ok)
        {
            ShowError(services.Error);
            return null;
        }

        Users = users.Data ?? [];
        Services = services.Data ?? [];
        IdUser = idUser is > 0 ? idUser.Value : Users.FirstOrDefault()?.IdUser ?? 0;
        IdService = idService is > 0 ? idService.Value : Services.FirstOrDefault()?.IdService ?? 0;

        if (IdService > 0)
        {
            var endpoints = await Api.GetAsync<List<EndpointRow>>($"api/Endpoints/ListByService?idService={IdService}", Token);
            denied = RedirectIfUnauthorized(endpoints.Status);
            if (denied is not null)
            {
                return denied;
            }

            if (!endpoints.Ok)
            {
                ShowError(endpoints.Error);
                return null;
            }

            Endpoints = endpoints.Data ?? [];
        }

        if (IdUser > 0)
        {
            var permissions = await Api.GetAsync<List<PermissionRow>>($"api/Permissions/ListByUser?idUser={IdUser}", Token);
            denied = RedirectIfUnauthorized(permissions.Status);
            if (denied is not null)
            {
                return denied;
            }

            if (!permissions.Ok)
            {
                ShowError(permissions.Error);
                return null;
            }

            Permissions = permissions.Data ?? [];
        }

        return null;
    }
}
