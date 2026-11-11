using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages.Endpoints;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public IReadOnlyList<ServiceRow> Services { get; private set; } = [];
    public IReadOnlyList<EndpointRow> Endpoints { get; private set; } = [];
    public int IdService { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? idService)
    {
        return await LoadAsync(idService) ?? Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(int idService, string name, string httpMethod, string route)
    {
        if (idService <= 0 || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(route))
        {
            ShowError("Servicio, nombre, método y ruta son obligatorios.");
            return await LoadAsync(idService) ?? Page();
        }

        var call = await Api.PostAsync<EndpointRow>("api/Endpoints/Create", new
        {
            idService,
            name = name.Trim(),
            httpMethod,
            route = route.Trim()
        }, Token);

        var denied = RedirectIfUnauthorized(call.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!call.Ok)
        {
            ShowError(call.Error);
            return await LoadAsync(idService) ?? Page();
        }

        TempData["Notice"] = "Endpoint creado.";
        return RedirectToPage(new { idService });
    }

    private async Task<IActionResult?> LoadAsync(int? idService)
    {
        var services = await Api.GetAsync<List<ServiceRow>>("api/Services/List", Token);
        var denied = RedirectIfUnauthorized(services.Status);
        if (denied is not null)
        {
            return denied;
        }

        if (!services.Ok)
        {
            ShowError(services.Error);
            return null;
        }

        Services = services.Data ?? [];
        IdService = idService is > 0 ? idService.Value : Services.FirstOrDefault()?.IdService ?? 0;
        if (IdService <= 0)
        {
            return null;
        }

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
        return null;
    }
}
