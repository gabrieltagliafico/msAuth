using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Endpoints;

public class EditModel : AdminPageModel
{
    public EditModel(AuthApiClient api) : base(api)
    {
    }

    public int IdEndpoint { get; private set; }
    public int IdService { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string HttpMethod { get; private set; } = "GET";
    public string Route { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, int idService)
    {
        return await LoadAsync(id, idService) ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, int idService, string name, string httpMethod, string route, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(route))
        {
            ShowError("Nombre, método y ruta son obligatorios.");
            return await LoadAsync(id, idService) ?? Page();
        }

        var call = await Api.PostAsync<EndpointRow>("api/Endpoints/Update", new
        {
            idEndpoint = id,
            name = name.Trim(),
            httpMethod,
            route = route.Trim(),
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
            return await LoadAsync(id, idService) ?? Page();
        }

        TempData["Notice"] = "Endpoint actualizado.";
        return RedirectToPage("Index", new { idService });
    }

    private async Task<IActionResult?> LoadAsync(int id, int idService)
    {
        var call = await Api.GetAsync<List<EndpointRow>>($"api/Endpoints/ListByService?idService={idService}", Token);
        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            TempData["Error"] = call.Error;
            return RedirectToPage("Index", new { idService });
        }

        var endpoint = call.Data?.FirstOrDefault(item => item.IdEndpoint == id);
        if (endpoint is null)
        {
            TempData["Error"] = "El endpoint no está en el listado.";
            return RedirectToPage("Index", new { idService });
        }

        IdEndpoint = endpoint.IdEndpoint;
        IdService = endpoint.IdService;
        Name = endpoint.Name ?? string.Empty;
        HttpMethod = endpoint.HttpMethod ?? "GET";
        Route = endpoint.Route ?? string.Empty;
        IsActive = endpoint.IsActive;
        return null;
    }
}
