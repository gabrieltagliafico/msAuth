using MsAuthAdmin.Models;
using MsAuthAdmin.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthApiOptions>(builder.Configuration.GetSection("AuthApi"));
builder.Services.AddHttpClient<AuthApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AuthApiOptions>>().Value;
    var baseUrl = options.BaseUrl.Trim();
    if (baseUrl.Length == 0)
    {
        return;
    }

    if (!baseUrl.EndsWith('/'))
    {
        baseUrl += "/";
    }

    if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
    {
        client.BaseAddress = uri;
    }

    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(12);
    options.Cookie.Name = ".MsAuthAdmin";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.ConfigureFilter(new AdminAuthorizeFilter());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
