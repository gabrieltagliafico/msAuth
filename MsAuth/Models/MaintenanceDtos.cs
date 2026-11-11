using System.Text.Json.Serialization;

namespace BNC.Microservices.Auth.Models
{
    public interface IHasResultCode
    {
        int ResultCode { get; set; }
    }

    public record MaintenanceResult<T>
    {
        public bool IsAuthorized { get; set; }
        public int ResultCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public record UserDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdUser { get; set; }
        public Guid UidUser { get; set; }
        public string? Name { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record ServiceDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdService { get; set; }
        public string? Name { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record EndpointDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdEndpoint { get; set; }
        public int IdService { get; set; }
        public string? Name { get; set; }
        public string? HttpMethod { get; set; }
        public string? Route { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record PermissionDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdPermission { get; set; }
        public int IdUser { get; set; }
        public int IdService { get; set; }
        public int IdEndpoint { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record RoleDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdRole { get; set; }
        public string? Name { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record UserRoleDto : IHasResultCode
    {
        [JsonIgnore]
        public int ResultCode { get; set; }
        public int IdUserRole { get; set; }
        public int IdUser { get; set; }
        public int IdRole { get; set; }
        public string? Name { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime? DtModified { get; set; }
        public bool IsActive { get; set; }
    }

    public record RoleNameDto
    {
        public string? Name { get; set; }
    }
}
