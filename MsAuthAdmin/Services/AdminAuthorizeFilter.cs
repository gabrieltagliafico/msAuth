using Microsoft.AspNetCore.Mvc.Filters;

namespace MsAuthAdmin.Services;

public sealed class AdminAuthorizeFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var page = context.ActionDescriptor.ViewEnginePath;
        if (page.Equals("/Login", StringComparison.OrdinalIgnoreCase)
            || page.Equals("/Logout", StringComparison.OrdinalIgnoreCase)
            || page.Equals("/Error", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        if (!AdminSession.IsActive(context.HttpContext.Session))
        {
            context.Result = new RedirectToPageResult("/Login");
            return;
        }

        await next();
    }
}
