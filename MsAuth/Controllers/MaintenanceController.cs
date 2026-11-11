using BNC.Microservices.Auth.Models;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BNC.Microservices.MsAuth.Controllers
{
    [ApiController]
    public abstract class MaintenanceController : ControllerBase
    {
        protected readonly IDBLayerDap DbLayer;

        protected MaintenanceController(IDBLayerDap dbLayer)
        {
            DbLayer = dbLayer;
        }

        protected string AuthorizationHeader => Request.Headers.Authorization.ToString();

        protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<MaintenanceResult<T>>> action, string operation)
        {
            try
            {
                var result = await action();
                if (!result.IsAuthorized)
                {
                    return Unauthorized(new { Message = "Credenciales o permisos inválidos." });
                }

                return result.ResultCode switch
                {
                    0 => Ok(result.Data),
                    1 => NotFound(new { Message = result.Message }),
                    2 => Conflict(new { Message = result.Message }),
                    _ => BadRequest(new { Message = result.Message })
                };
            }
            catch (Exception ex)
            {
                await DbLayer.SaveExceptionAsync(ex, HttpContext.Request.GetDisplayUrl(), operation);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Ocurrió un error interno procesando la solicitud." });
            }
        }
    }
}
