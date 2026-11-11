using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Models;

namespace BNC.Microservices.MsAuth.Controllers
{
    [Route("api/[controller]")]
    public class ServicesController : MaintenanceController
    {
        private readonly IAuthService _authService;

        public ServicesController(IAuthService authService, IDBLayerDap dbLayer) : base(dbLayer)
        {
            _authService = authService;
        }

        /// <summary>
        /// Crea un servicio. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Create([FromBody] ServiceCreateRequest model)
        {
            return ExecuteAsync(
                () => _authService.CreateServiceAsync(AuthorizationHeader, model.Name),
                "AuthService.CreateServiceAsync");
        }

        /// <summary>
        /// Modifica el nombre y el estado de un servicio. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Update([FromBody] ServiceUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdateServiceAsync(AuthorizationHeader, model.IdService, model.Name, model.IsActive!.Value),
                "AuthService.UpdateServiceAsync");
        }

        /// <summary>
        /// Lista los servicios. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("List")]
        [ProducesResponseType(typeof(IReadOnlyList<ServiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> List()
        {
            return ExecuteAsync(
                () => _authService.ListServicesAsync(AuthorizationHeader),
                "AuthService.ListServicesAsync");
        }
    }
}
