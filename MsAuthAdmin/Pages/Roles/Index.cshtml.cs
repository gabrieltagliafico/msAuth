using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Roles;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public IReadOnlyList<RoleRow> Roles { get; private set; } = [];
    public IReadOnlyList<UserRow> Users { get; private set; } = [];
    public IReadOnlyList<UserRoleRow> Assignments { get; private set; } = [];
    public int IdUser { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? idUser)
    {
        return await LoadAsync(idUser) ?? Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string name, int? idUser)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("El nombre es obligatorio.");
            return await LoadAsync(idUser) ?? Page();
        }

        var call = await Api.PostAsync<RoleRow>("api/Roles/Create", new { name = name.Trim() }, Token);
        return await FinishAsync(call, idUser, "Rol creado.");
    }

    public async Task<IActionResult> OnPostUpdateAsync(int idRole, string name, bool isActive, int? idUser)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("El nombre es obligatorio.");
            return await LoadAsync(idUser) ?? Page();
        }

        var call = await Api.PostAsync<RoleRow>("api/Roles/Update", new { idRole, name = name.Trim(), isActive }, Token);
        return await FinishAsync(call, idUser, "Rol actualizado.");
    }

    public async Task<IActionResult> OnPostAssignAsync(int idUser, int idRole)
    {
        var call = await Api.PostAsync<UserRoleRow>("api/Roles/Assign", new { idUser, idRole }, Token);
        return await FinishAsync(call, idUser, "Rol asignado.");
    }

    public async Task<IActionResult> OnPostUpdateAssignmentAsync(int idUserRole, bool isActive, int idUser)
    {
        var call = await Api.PostAsync<UserRoleRow>("api/Roles/UpdateAssignment", new { idUserRole, isActive }, Token);
        return await FinishAsync(call, idUser, "Asignación actualizada.");
    }

    private async Task<IActionResult> FinishAsync<T>(ApiCall<T> call, int? idUser, string notice)
    {
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync(idUser) ?? Page();
        }

        TempData["Notice"] = notice;
        return RedirectToPage(new { idUser });
    }

    private async Task<IActionResult?> LoadAsync(int? idUser)
    {
        var roles = await Api.GetAsync<List<RoleRow>>("api/Roles/List", Token);
        var denied = RedirectIfUnauthorized(roles.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!roles.Ok)
        {
            ShowError(roles.Error);
            return null;
        }

        var users = await Api.GetAsync<List<UserRow>>("api/Users/List", Token);
        denied = RedirectIfUnauthorized(users.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!users.Ok)
        {
            ShowError(users.Error);
            return null;
        }

        Roles = roles.Data ?? [];
        Users = users.Data ?? [];
        IdUser = idUser is > 0 ? idUser.Value : Users.FirstOrDefault()?.IdUser ?? 0;
        if (IdUser <= 0)
        {
            return null;
        }

        var assignments = await Api.GetAsync<List<UserRoleRow>>($"api/Roles/ListByUser?idUser={IdUser}", Token);
        denied = RedirectIfUnauthorized(assignments.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!assignments.Ok)
        {
            ShowError(assignments.Error);
            return null;
        }

        Assignments = assignments.Data ?? [];
        return null;
    }
}
