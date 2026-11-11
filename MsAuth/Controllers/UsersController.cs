using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Models;

namespace BNC.Microservices.MsAuth.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : MaintenanceController
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService, IDBLayerDap dbLayer) : base(dbLayer)
        {
            _authService = authService;
        }

        /// <summary>
        /// Crea un usuario y guarda el hash de su API key. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Create([FromBody] UserCreateRequest model)
        {
            return ExecuteAsync(
                () => _authService.CreateUserAsync(AuthorizationHeader, model.Name, model.ApiKey),
                "AuthService.CreateUserAsync");
        }

        /// <summary>
        /// Modifica el nombre, el estado y, si se envía, la API key de un usuario. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Update([FromBody] UserUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdateUserAsync(AuthorizationHeader, model.IdUser, model.Name, model.IsActive!.Value, model.ApiKey),
                "AuthService.UpdateUserAsync");
        }

        /// <summary>
        /// Lista los usuarios sin exponer la API key. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("List")]
        [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> List()
        {
            return ExecuteAsync(
                () => _authService.ListUsersAsync(AuthorizationHeader),
                "AuthService.ListUsersAsync");
        }
    }
}
