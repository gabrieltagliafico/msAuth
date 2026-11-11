using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Models;

namespace BNC.Microservices.MsAuth.Controllers
{
    [Route("api/[controller]")]
    public class PermissionsController : MaintenanceController
    {
        private readonly IAuthService _authService;

        public PermissionsController(IAuthService authService, IDBLayerDap dbLayer) : base(dbLayer)
        {
            _authService = authService;
        }

        /// <summary>
        /// Valida la API key del usuario, empaqueta los endpoints que puede ejecutar en el servicio
        /// y devuelve un JWT vigente por 24 horas junto con su bearer.
        /// </summary>
        [HttpPost("IssueToken")]
        [ProducesResponseType(typeof(IssueTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IssueToken([FromBody] IssueTokenRequest model)
        {
            try
            {
                var result = await _authService.IssueTokenAsync(model.UidUser, model.ApiKey, model.IdService);
                if (!result.IsAuthorized || result.Token is null)
                {
                    return Unauthorized(new { Message = "Credenciales o permisos inválidos." });
                }

                return Ok(result.Token);
            }
            catch (Exception ex)
            {
                await DbLayer.SaveExceptionAsync(ex, HttpContext.Request.GetDisplayUrl(), "AuthService.IssueTokenAsync");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Ocurrió un error interno procesando la solicitud." });
            }
        }

        /// <summary>
        /// Autoriza a un usuario a ejecutar un endpoint de un servicio. Si el permiso ya existe, lo reactiva.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Create([FromBody] PermissionCreateRequest model)
        {
            return ExecuteAsync(
                () => _authService.CreatePermissionAsync(AuthorizationHeader, model.IdUser, model.IdService, model.IdEndpoint),
                "AuthService.CreatePermissionAsync");
        }

        /// <summary>
        /// Activa o desactiva un permiso. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Update([FromBody] PermissionUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdatePermissionAsync(AuthorizationHeader, model.IdPermission, model.IsActive!.Value),
                "AuthService.UpdatePermissionAsync");
        }

        /// <summary>
        /// Lista los permisos de un usuario. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("ListByUser")]
        [ProducesResponseType(typeof(IReadOnlyList<PermissionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ListByUser([FromQuery] PermissionListByUserRequest model)
        {
            return ExecuteAsync(
                () => _authService.ListPermissionsByUserAsync(AuthorizationHeader, model.IdUser),
                "AuthService.ListPermissionsByUserAsync");
        }
    }
}
