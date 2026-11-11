using MsAuthAdmin.Services;

namespace MsAuthAdmin.Pages;

public class IndexModel : AdminPageModel
{
    public IndexModel(AuthApiClient api) : base(api)
    {
    }

    public string? UserId { get; private set; }
    public string ExpiresText { get; private set; } = string.Empty;

    public void OnGet()
    {
        UserId = AdminSession.UserId(HttpContext.Session);
        var expires = AdminSession.GetExpires(HttpContext.Session);
        if (expires is not null)
        {
            ExpiresText = expires.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        }
    }
}
