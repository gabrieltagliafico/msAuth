using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Models;

namespace BNC.Microservices.MsAuth.Controllers
{
    [Route("api/[controller]")]
    public class EndpointsController : MaintenanceController
    {
        private readonly IAuthService _authService;

        public EndpointsController(IAuthService authService, IDBLayerDap dbLayer) : base(dbLayer)
        {
            _authService = authService;
        }

        /// <summary>
        /// Crea un endpoint dentro de un servicio. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Create([FromBody] EndpointCreateRequest model)
        {
            return ExecuteAsync(
                () => _authService.CreateEndpointAsync(AuthorizationHeader, model.IdService, model.Name, model.HttpMethod, model.Route),
                "AuthService.CreateEndpointAsync");
        }

        /// <summary>
        /// Modifica el nombre, el método, la ruta y el estado de un endpoint. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Update([FromBody] EndpointUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdateEndpointAsync(
                    AuthorizationHeader,
                    model.IdEndpoint,
                    model.Name,
                    model.HttpMethod,
                    model.Route,
                    model.IsActive!.Value),
                "AuthService.UpdateEndpointAsync");
        }

        /// <summary>
        /// Lista los endpoints de un servicio. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("ListByService")]
        [ProducesResponseType(typeof(IReadOnlyList<EndpointDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ListByService([FromQuery] EndpointListByServiceRequest model)
        {
            return ExecuteAsync(
                () => _authService.ListEndpointsByServiceAsync(AuthorizationHeader, model.IdService),
                "AuthService.ListEndpointsByServiceAsync");
        }
    }
}
