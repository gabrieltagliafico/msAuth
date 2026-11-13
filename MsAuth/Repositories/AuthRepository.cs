using BNC.Microservices.Auth.Models;
using BNC.Microservices.Infrastructure;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;

namespace BNC.Microservices.Auth.Repositories
{
    public interface IAuthRepository
    {
        Task<IEnumerable<PermissionEndpointDto>> GetUserPermissionsAsync(Guid uidUser, string apiKey, int idService);

        Task<IEnumerable<RoleNameDto>> GetUserRolesAsync(int idUser);

        Task<IEnumerable<AccessTokenDto>> SaveAccessTokenAsync(int idUser, int idService, string token, DateTime dtExpires);

        Task<IEnumerable<UserDto>> InsertUserAsync(string name, string apiKey);

        Task<IEnumerable<UserDto>> UpdateUserAsync(int idUser, string name, bool isActive, string? apiKey);

        Task<IEnumerable<UserDto>> ListUsersAsync();

        Task<IEnumerable<UserDto>> DeleteUserAsync(int idUser);

        Task<IEnumerable<ServiceDto>> InsertServiceAsync(string name);

        Task<IEnumerable<ServiceDto>> UpdateServiceAsync(int idService, string name, bool isActive);

        Task<IEnumerable<ServiceDto>> ListServicesAsync();

        Task<IEnumerable<EndpointDto>> InsertEndpointAsync(int idService, string name, string httpMethod, string route);

        Task<IEnumerable<EndpointDto>> UpdateEndpointAsync(int idEndpoint, string name, string httpMethod, string route, bool isActive);

        Task<IEnumerable<EndpointDto>> ListEndpointsByServiceAsync(int idService);

        Task<IEnumerable<PermissionDto>> InsertPermissionAsync(int idUser, int idService, int idEndpoint);

        Task<IEnumerable<PermissionDto>> UpdatePermissionAsync(int idPermission, bool isActive);

        Task<IEnumerable<PermissionDto>> ListPermissionsByUserAsync(int idUser);

        Task<IEnumerable<RoleDto>> InsertRoleAsync(string name);

        Task<IEnumerable<RoleDto>> UpdateRoleAsync(int idRole, string name, bool isActive);

        Task<IEnumerable<RoleDto>> ListRolesAsync();

        Task<IEnumerable<UserRoleDto>> AssignUserRoleAsync(int idUser, int idRole);

        Task<IEnumerable<UserRoleDto>> UpdateUserRoleAsync(int idUserRole, bool isActive);

        Task<IEnumerable<UserRoleDto>> ListUserRolesByUserAsync(int idUser);
    }

    public class AuthRepository : IAuthRepository
    {
        private readonly IDBLayerDap _dbLayer;
        private readonly RepositorySettings _repSettings;

        private const string ConnectionName = "BNCAuth";

        public AuthRepository(IDBLayerDap dbLayer, IOptions<RepositorySettings> repSettings)
        {
            _dbLayer = dbLayer;
            _repSettings = repSettings.Value;
        }

        public Task<IEnumerable<PermissionEndpointDto>> GetUserPermissionsAsync(Guid uidUser, string apiKey, int idService)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@UidUser", uidUser, DbType.Guid);
            parameters.Add("@ApiKey", apiKey, DbType.String, size: 256);
            parameters.Add("@idService", idService, DbType.Int32);
            return QueryAsync<PermissionEndpointDto>("GetUserPermissions", parameters, "Error de base de datos al consultar permisos");
        }

        public Task<IEnumerable<RoleNameDto>> GetUserRolesAsync(int idUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            return QueryAsync<RoleNameDto>("GetUserRoles", parameters, "Error de base de datos al consultar roles");
        }

        public Task<IEnumerable<AccessTokenDto>> SaveAccessTokenAsync(int idUser, int idService, string token, DateTime dtExpires)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            parameters.Add("@idService", idService, DbType.Int32);
            parameters.Add("@token", token, DbType.String);
            parameters.Add("@dtExpires", dtExpires, DbType.DateTime2);
            return QueryAsync<AccessTokenDto>("SaveAccessToken", parameters, "Error de base de datos al almacenar el token");
        }

        public Task<IEnumerable<UserDto>> InsertUserAsync(string name, string apiKey)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@name", name, DbType.String, size: 150);
            parameters.Add("@apiKey", apiKey, DbType.String, size: 256);
            return QueryAsync<UserDto>("InsertUser", parameters, "Error de base de datos al crear el usuario");
        }

        public Task<IEnumerable<UserDto>> UpdateUserAsync(int idUser, string name, bool isActive, string? apiKey)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            parameters.Add("@name", name, DbType.String, size: 150);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            parameters.Add("@apiKey", string.IsNullOrEmpty(apiKey) ? null : apiKey, DbType.String, size: 256);
            return QueryAsync<UserDto>("UpdateUser", parameters, "Error de base de datos al modificar el usuario");
        }

        public Task<IEnumerable<UserDto>> ListUsersAsync()
        {
            return QueryAsync<UserDto>("ListUsers", new DynamicParameters(), "Error de base de datos al listar usuarios");
        }

        public Task<IEnumerable<UserDto>> DeleteUserAsync(int idUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            return QueryAsync<UserDto>("DeleteUser", parameters, "Error de base de datos al eliminar el usuario");
        }

        public Task<IEnumerable<ServiceDto>> InsertServiceAsync(string name)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@name", name, DbType.String, size: 150);
            return QueryAsync<ServiceDto>("InsertService", parameters, "Error de base de datos al crear el servicio");
        }

        public Task<IEnumerable<ServiceDto>> UpdateServiceAsync(int idService, string name, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idService", idService, DbType.Int32);
            parameters.Add("@name", name, DbType.String, size: 150);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            return QueryAsync<ServiceDto>("UpdateService", parameters, "Error de base de datos al modificar el servicio");
        }

        public Task<IEnumerable<ServiceDto>> ListServicesAsync()
        {
            return QueryAsync<ServiceDto>("ListServices", new DynamicParameters(), "Error de base de datos al listar servicios");
        }

        public Task<IEnumerable<EndpointDto>> InsertEndpointAsync(int idService, string name, string httpMethod, string route)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idService", idService, DbType.Int32);
            parameters.Add("@name", name, DbType.String, size: 150);
            parameters.Add("@httpMethod", httpMethod, DbType.AnsiString, size: 10);
            parameters.Add("@route", route, DbType.String, size: 300);
            return QueryAsync<EndpointDto>("InsertEndpoint", parameters, "Error de base de datos al crear el endpoint");
        }

        public Task<IEnumerable<EndpointDto>> UpdateEndpointAsync(int idEndpoint, string name, string httpMethod, string route, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idEndpoint", idEndpoint, DbType.Int32);
            parameters.Add("@name", name, DbType.String, size: 150);
            parameters.Add("@httpMethod", httpMethod, DbType.AnsiString, size: 10);
            parameters.Add("@route", route, DbType.String, size: 300);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            return QueryAsync<EndpointDto>("UpdateEndpoint", parameters, "Error de base de datos al modificar el endpoint");
        }

        public Task<IEnumerable<EndpointDto>> ListEndpointsByServiceAsync(int idService)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idService", idService, DbType.Int32);
            return QueryAsync<EndpointDto>("ListEndpointsByService", parameters, "Error de base de datos al listar endpoints");
        }

        public Task<IEnumerable<PermissionDto>> InsertPermissionAsync(int idUser, int idService, int idEndpoint)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            parameters.Add("@idService", idService, DbType.Int32);
            parameters.Add("@idEndpoint", idEndpoint, DbType.Int32);
            return QueryAsync<PermissionDto>("InsertPermission", parameters, "Error de base de datos al crear el permiso");
        }

        public Task<IEnumerable<PermissionDto>> UpdatePermissionAsync(int idPermission, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idPermission", idPermission, DbType.Int32);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            return QueryAsync<PermissionDto>("UpdatePermission", parameters, "Error de base de datos al modificar el permiso");
        }

        public Task<IEnumerable<PermissionDto>> ListPermissionsByUserAsync(int idUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            return QueryAsync<PermissionDto>("ListPermissionsByUser", parameters, "Error de base de datos al listar permisos");
        }

        public Task<IEnumerable<RoleDto>> InsertRoleAsync(string name)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@name", name, DbType.String, size: 100);
            return QueryAsync<RoleDto>("InsertRole", parameters, "Error de base de datos al crear el rol");
        }

        public Task<IEnumerable<RoleDto>> UpdateRoleAsync(int idRole, string name, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idRole", idRole, DbType.Int32);
            parameters.Add("@name", name, DbType.String, size: 100);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            return QueryAsync<RoleDto>("UpdateRole", parameters, "Error de base de datos al modificar el rol");
        }

        public Task<IEnumerable<RoleDto>> ListRolesAsync()
        {
            return QueryAsync<RoleDto>("ListRoles", new DynamicParameters(), "Error de base de datos al listar roles");
        }

        public Task<IEnumerable<UserRoleDto>> AssignUserRoleAsync(int idUser, int idRole)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            parameters.Add("@idRole", idRole, DbType.Int32);
            return QueryAsync<UserRoleDto>("AssignUserRole", parameters, "Error de base de datos al asignar el rol");
        }

        public Task<IEnumerable<UserRoleDto>> UpdateUserRoleAsync(int idUserRole, bool isActive)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUserRole", idUserRole, DbType.Int32);
            parameters.Add("@isActive", isActive, DbType.Boolean);
            return QueryAsync<UserRoleDto>("UpdateUserRole", parameters, "Error de base de datos al modificar la asignación del rol");
        }

        public Task<IEnumerable<UserRoleDto>> ListUserRolesByUserAsync(int idUser)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@idUser", idUser, DbType.Int32);
            return QueryAsync<UserRoleDto>("ListUserRolesByUser", parameters, "Error de base de datos al listar roles del usuario");
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string procedureKey, DynamicParameters parameters, string error)
        {
            var response = await _dbLayer.QueryListAsync<T>(
                _repSettings.BNCAuth.StoredProcedures[procedureKey],
                parameters,
                ConnectionName);

            if (!response.IsSuccess)
            {
                throw new ApplicationException($"{error}: {response.Message}");
            }

            return response.Content ?? [];
        }
    }
}
