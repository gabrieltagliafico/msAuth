using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Services;
using BNC.Microservices.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using MsAuth.Models;

namespace BNC.Microservices.MsAuth.Controllers
{
    [Route("api/[controller]")]
    public class RolesController : MaintenanceController
    {
        private readonly IAuthService _authService;

        public RolesController(IAuthService authService, IDBLayerDap dbLayer) : base(dbLayer)
        {
            _authService = authService;
        }

        /// <summary>
        /// Crea un rol. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("Create")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Create([FromBody] RoleCreateRequest model)
        {
            return ExecuteAsync(
                () => _authService.CreateRoleAsync(AuthorizationHeader, model.Name),
                "AuthService.CreateRoleAsync");
        }

        /// <summary>
        /// Modifica el nombre y el estado de un rol. El rol Administrator no se puede renombrar ni desactivar.
        /// </summary>
        [HttpPost("Update")]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Update([FromBody] RoleUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdateRoleAsync(AuthorizationHeader, model.IdRole, model.Name, model.IsActive!.Value),
                "AuthService.UpdateRoleAsync");
        }

        /// <summary>
        /// Lista los roles. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("List")]
        [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> List()
        {
            return ExecuteAsync(
                () => _authService.ListRolesAsync(AuthorizationHeader),
                "AuthService.ListRolesAsync");
        }

        /// <summary>
        /// Asigna un rol activo a un usuario. Si la asignación ya existe, la reactiva.
        /// </summary>
        [HttpPost("Assign")]
        [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Assign([FromBody] UserRoleAssignRequest model)
        {
            return ExecuteAsync(
                () => _authService.AssignUserRoleAsync(AuthorizationHeader, model.IdUser, model.IdRole),
                "AuthService.AssignUserRoleAsync");
        }

        /// <summary>
        /// Activa o desactiva la asignación de un rol a un usuario. Requiere el rol Administrator.
        /// </summary>
        [HttpPost("UpdateAssignment")]
        [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> UpdateAssignment([FromBody] UserRoleUpdateRequest model)
        {
            return ExecuteAsync(
                () => _authService.UpdateUserRoleAsync(AuthorizationHeader, model.IdUserRole, model.IsActive!.Value),
                "AuthService.UpdateUserRoleAsync");
        }

        /// <summary>
        /// Lista los roles asignados a un usuario. Requiere el rol Administrator.
        /// </summary>
        [HttpGet("ListByUser")]
        [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ListByUser([FromQuery] UserRoleListByUserRequest model)
        {
            return ExecuteAsync(
                () => _authService.ListUserRolesByUserAsync(AuthorizationHeader, model.IdUser),
                "AuthService.ListUserRolesByUserAsync");
        }
    }
}
