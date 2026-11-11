using BNC.Microservices.Auth.Models;
using BNC.Microservices.Auth.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BNC.Microservices.Auth.Services
{
    public interface IAuthService
    {
        Task<IssueTokenResult> IssueTokenAsync(Guid uidUser, string apiKey, int idService);

        Task<MaintenanceResult<UserDto>> CreateUserAsync(string? authorization, string name, string apiKey);

        Task<MaintenanceResult<UserDto>> UpdateUserAsync(string? authorization, int idUser, string name, bool isActive, string? apiKey);

        Task<MaintenanceResult<IReadOnlyList<UserDto>>> ListUsersAsync(string? authorization);

        Task<MaintenanceResult<ServiceDto>> CreateServiceAsync(string? authorization, string name);

        Task<MaintenanceResult<ServiceDto>> UpdateServiceAsync(string? authorization, int idService, string name, bool isActive);

        Task<MaintenanceResult<IReadOnlyList<ServiceDto>>> ListServicesAsync(string? authorization);

        Task<MaintenanceResult<EndpointDto>> CreateEndpointAsync(string? authorization, int idService, string name, string httpMethod, string route);

        Task<MaintenanceResult<EndpointDto>> UpdateEndpointAsync(string? authorization, int idEndpoint, string name, string httpMethod, string route, bool isActive);

        Task<MaintenanceResult<IReadOnlyList<EndpointDto>>> ListEndpointsByServiceAsync(string? authorization, int idService);

        Task<MaintenanceResult<PermissionDto>> CreatePermissionAsync(string? authorization, int idUser, int idService, int idEndpoint);

        Task<MaintenanceResult<PermissionDto>> UpdatePermissionAsync(string? authorization, int idPermission, bool isActive);

        Task<MaintenanceResult<IReadOnlyList<PermissionDto>>> ListPermissionsByUserAsync(string? authorization, int idUser);

        Task<MaintenanceResult<RoleDto>> CreateRoleAsync(string? authorization, string name);

        Task<MaintenanceResult<RoleDto>> UpdateRoleAsync(string? authorization, int idRole, string name, bool isActive);

        Task<MaintenanceResult<IReadOnlyList<RoleDto>>> ListRolesAsync(string? authorization);

        Task<MaintenanceResult<UserRoleDto>> AssignUserRoleAsync(string? authorization, int idUser, int idRole);

        Task<MaintenanceResult<UserRoleDto>> UpdateUserRoleAsync(string? authorization, int idUserRole, bool isActive);

        Task<MaintenanceResult<IReadOnlyList<UserRoleDto>>> ListUserRolesByUserAsync(string? authorization, int idUser);
    }

    public class AuthService : IAuthService
    {
        private const int AuthorizedResultCode = 0;
        private const int NoEndpointsResultCode = 3;
        private const int MinimumSigningKeyLength = 32;
        private const string AdministratorRole = "Administrator";

        private readonly IAuthRepository _authRepository;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IAuthRepository authRepository, IOptions<JwtSettings> jwtSettings)
        {
            _authRepository = authRepository;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<IssueTokenResult> IssueTokenAsync(Guid uidUser, string apiKey, int idService)
        {
            var permissions = (await _authRepository.GetUserPermissionsAsync(uidUser, apiKey, idService))?.ToList()
                ?? [];

            if (permissions.Count == 0)
            {
                return UnauthorizedToken();
            }

            var granted = permissions[0];
            if (granted.ResultCode is not (AuthorizedResultCode or NoEndpointsResultCode))
            {
                return UnauthorizedToken();
            }

            if (granted.IdUser <= 0 || granted.UidUser == Guid.Empty || granted.IdService <= 0)
            {
                return UnauthorizedToken();
            }

            var endpoints = permissions
                .Where(permission =>
                    permission.ResultCode == AuthorizedResultCode
                    && permission.IdEndpoint is > 0
                    && !string.IsNullOrWhiteSpace(permission.HttpMethod)
                    && !string.IsNullOrWhiteSpace(permission.Route))
                .ToList();

            var roles = (await _authRepository.GetUserRolesAsync(granted.IdUser))
                .Select(role => role.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (endpoints.Count == 0 && roles.Count == 0)
            {
                return UnauthorizedToken();
            }

            EnsureSigningKey();

            var lifetimeHours = _jwtSettings.LifetimeHours > 0 ? _jwtSettings.LifetimeHours : 24;
            var issuedAt = TruncateToSeconds(DateTime.UtcNow);
            var expires = issuedAt.AddHours(lifetimeHours);
            var jwt = CreateJwt(granted, endpoints, roles, issuedAt, expires);

            var savedRows = await _authRepository.SaveAccessTokenAsync(granted.IdUser, granted.IdService, jwt, expires);
            var saved = savedRows?.FirstOrDefault();
            if (saved is null)
            {
                throw new ApplicationException("Error de base de datos al almacenar el token: el procedure no devolvió la fila insertada.");
            }

            return new IssueTokenResult
            {
                IsAuthorized = true,
                Token = new IssueTokenResponse
                {
                    Token = jwt,
                    Bearer = $"Bearer {jwt}",
                    DtExpires = saved.DtExpires
                }
            };
        }

        public async Task<MaintenanceResult<UserDto>> CreateUserAsync(string? authorization, string name, string apiKey)
        {
            var denied = Gate<UserDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.InsertUserAsync(name.Trim(), apiKey));
        }

        public async Task<MaintenanceResult<UserDto>> UpdateUserAsync(string? authorization, int idUser, string name, bool isActive, string? apiKey)
        {
            var denied = Gate<UserDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            var nextKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey;
            return FromRow(await _authRepository.UpdateUserAsync(idUser, name.Trim(), isActive, nextKey));
        }

        public async Task<MaintenanceResult<IReadOnlyList<UserDto>>> ListUsersAsync(string? authorization)
        {
            var denied = Gate<IReadOnlyList<UserDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<UserDto>)(await _authRepository.ListUsersAsync()).ToList());
        }

        public async Task<MaintenanceResult<ServiceDto>> CreateServiceAsync(string? authorization, string name)
        {
            var denied = Gate<ServiceDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.InsertServiceAsync(name.Trim()));
        }

        public async Task<MaintenanceResult<ServiceDto>> UpdateServiceAsync(string? authorization, int idService, string name, bool isActive)
        {
            var denied = Gate<ServiceDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.UpdateServiceAsync(idService, name.Trim(), isActive));
        }

        public async Task<MaintenanceResult<IReadOnlyList<ServiceDto>>> ListServicesAsync(string? authorization)
        {
            var denied = Gate<IReadOnlyList<ServiceDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<ServiceDto>)(await _authRepository.ListServicesAsync()).ToList());
        }

        public async Task<MaintenanceResult<EndpointDto>> CreateEndpointAsync(string? authorization, int idService, string name, string httpMethod, string route)
        {
            var denied = Gate<EndpointDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.InsertEndpointAsync(idService, name.Trim(), NormalizeMethod(httpMethod), route.Trim()));
        }

        public async Task<MaintenanceResult<EndpointDto>> UpdateEndpointAsync(string? authorization, int idEndpoint, string name, string httpMethod, string route, bool isActive)
        {
            var denied = Gate<EndpointDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.UpdateEndpointAsync(idEndpoint, name.Trim(), NormalizeMethod(httpMethod), route.Trim(), isActive));
        }

        public async Task<MaintenanceResult<IReadOnlyList<EndpointDto>>> ListEndpointsByServiceAsync(string? authorization, int idService)
        {
            var denied = Gate<IReadOnlyList<EndpointDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<EndpointDto>)(await _authRepository.ListEndpointsByServiceAsync(idService)).ToList());
        }

        public async Task<MaintenanceResult<PermissionDto>> CreatePermissionAsync(string? authorization, int idUser, int idService, int idEndpoint)
        {
            var denied = Gate<PermissionDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(
                await _authRepository.InsertPermissionAsync(idUser, idService, idEndpoint),
                "El endpoint no pertenece al servicio indicado.");
        }

        public async Task<MaintenanceResult<PermissionDto>> UpdatePermissionAsync(string? authorization, int idPermission, bool isActive)
        {
            var denied = Gate<PermissionDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.UpdatePermissionAsync(idPermission, isActive));
        }

        public async Task<MaintenanceResult<IReadOnlyList<PermissionDto>>> ListPermissionsByUserAsync(string? authorization, int idUser)
        {
            var denied = Gate<IReadOnlyList<PermissionDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<PermissionDto>)(await _authRepository.ListPermissionsByUserAsync(idUser)).ToList());
        }

        public async Task<MaintenanceResult<RoleDto>> CreateRoleAsync(string? authorization, string name)
        {
            var denied = Gate<RoleDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.InsertRoleAsync(name.Trim()));
        }

        public async Task<MaintenanceResult<RoleDto>> UpdateRoleAsync(string? authorization, int idRole, string name, bool isActive)
        {
            var denied = Gate<RoleDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.UpdateRoleAsync(idRole, name.Trim(), isActive));
        }

        public async Task<MaintenanceResult<IReadOnlyList<RoleDto>>> ListRolesAsync(string? authorization)
        {
            var denied = Gate<IReadOnlyList<RoleDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<RoleDto>)(await _authRepository.ListRolesAsync()).ToList());
        }

        public async Task<MaintenanceResult<UserRoleDto>> AssignUserRoleAsync(string? authorization, int idUser, int idRole)
        {
            var denied = Gate<UserRoleDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(
                await _authRepository.AssignUserRoleAsync(idUser, idRole),
                "El rol no está activo.");
        }

        public async Task<MaintenanceResult<UserRoleDto>> UpdateUserRoleAsync(string? authorization, int idUserRole, bool isActive)
        {
            var denied = Gate<UserRoleDto>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return FromRow(await _authRepository.UpdateUserRoleAsync(idUserRole, isActive));
        }

        public async Task<MaintenanceResult<IReadOnlyList<UserRoleDto>>> ListUserRolesByUserAsync(string? authorization, int idUser)
        {
            var denied = Gate<IReadOnlyList<UserRoleDto>>(authorization);
            if (denied is not null)
            {
                return denied;
            }

            return Succeeded((IReadOnlyList<UserRoleDto>)(await _authRepository.ListUserRolesByUserAsync(idUser)).ToList());
        }

        private MaintenanceResult<T>? Gate<T>(string? authorization)
        {
            return IsAdministrator(authorization) ? null : Denied<T>();
        }

        private bool IsAdministrator(string? authorization)
        {
            var token = ReadBearer(authorization);
            if (token is null)
            {
                return false;
            }

            EnsureSigningKey();

            try
            {
                var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
                var principal = handler.ValidateToken(token, BuildValidationParameters(), out _);
                return principal.Claims.Any(claim =>
                    claim.Type == "role"
                    && string.Equals(claim.Value, AdministratorRole, StringComparison.Ordinal));
            }
            catch (Exception exception) when (exception is SecurityTokenException or ArgumentException)
            {
                return false;
            }
        }

        private static string? ReadBearer(string? authorization)
        {
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return null;
            }

            const string prefix = "Bearer ";
            var token = authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? authorization[prefix.Length..]
                : authorization;

            token = token.Trim();
            return token.Length == 0 ? null : token;
        }

        private TokenValidationParameters BuildValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        }

        private static MaintenanceResult<T> Denied<T>()
        {
            return new MaintenanceResult<T>
            {
                IsAuthorized = false,
                Message = "Credenciales o permisos inválidos."
            };
        }

        private static MaintenanceResult<T> Succeeded<T>(T data)
        {
            return new MaintenanceResult<T>
            {
                IsAuthorized = true,
                ResultCode = 0,
                Data = data
            };
        }

        private static MaintenanceResult<T> FromRow<T>(IEnumerable<T> rows, string? invalidMessage = null)
            where T : class, IHasResultCode
        {
            var row = rows.FirstOrDefault();
            if (row is null)
            {
                throw new ApplicationException("Error de base de datos: el procedure no devolvió un resultado.");
            }

            var message = row.ResultCode switch
            {
                0 => string.Empty,
                1 => "El registro solicitado no existe.",
                2 => "Ya existe un registro con esos datos.",
                4 => "El rol Administrator no se puede renombrar ni desactivar.",
                _ => invalidMessage ?? "La solicitud no es válida para el mantenimiento."
            };

            return new MaintenanceResult<T>
            {
                IsAuthorized = true,
                ResultCode = row.ResultCode,
                Message = message,
                Data = row.ResultCode == 0 ? row : null
            };
        }

        private void EnsureSigningKey()
        {
            if (string.IsNullOrWhiteSpace(_jwtSettings.SigningKey) || _jwtSettings.SigningKey.Length < MinimumSigningKeyLength)
            {
                throw new ApplicationException("La clave de firma JWT no está configurada o no cumple el largo mínimo.");
            }
        }

        private string CreateJwt(
            PermissionEndpointDto granted,
            IReadOnlyList<PermissionEndpointDto> endpoints,
            IReadOnlyList<string> roles,
            DateTime issuedAt,
            DateTime expires)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, granted.UidUser.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("serviceId", granted.IdService.ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            foreach (var endpoint in endpoints)
            {
                claims.Add(new Claim("endpoint", $"{endpoint.HttpMethod} {endpoint.Route}"));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                notBefore: issuedAt,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static IssueTokenResult UnauthorizedToken()
        {
            return new IssueTokenResult { IsAuthorized = false };
        }

        private static string NormalizeMethod(string httpMethod)
        {
            return httpMethod.Trim().ToUpperInvariant();
        }

        private static DateTime TruncateToSeconds(DateTime value)
        {
            return new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
        }
    }
}
