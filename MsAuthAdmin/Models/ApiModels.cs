namespace MsAuthAdmin.Models;

public sealed class AuthApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int ServiceId { get; set; }
}

public sealed class ApiCall<T>
{
    public bool Ok { get; init; }
    public int Status { get; init; }
    public T? Data { get; init; }
    public string Error { get; init; } = string.Empty;

    public static ApiCall<T> Success(T? data) => new() { Ok = true, Status = StatusCodes.Status200OK, Data = data };

    public static ApiCall<T> Fail(int status, string error) => new() { Status = status, Error = error };
}

public sealed class IssueTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime DtExpires { get; set; }
}

public sealed class UserRow
{
    public int IdUser { get; set; }
    public Guid UidUser { get; set; }
    public string? Name { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}

public sealed class ServiceRow
{
    public int IdService { get; set; }
    public string? Name { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}

public sealed class EndpointRow
{
    public int IdEndpoint { get; set; }
    public int IdService { get; set; }
    public string? Name { get; set; }
    public string? HttpMethod { get; set; }
    public string? Route { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}

public sealed class PermissionRow
{
    public int IdPermission { get; set; }
    public int IdUser { get; set; }
    public int IdService { get; set; }
    public int IdEndpoint { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}

public sealed class RoleRow
{
    public int IdRole { get; set; }
    public string? Name { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UserRoleRow
{
    public int IdUserRole { get; set; }
    public int IdUser { get; set; }
    public int IdRole { get; set; }
    public string? Name { get; set; }
    public DateTime DtCreated { get; set; }
    public DateTime? DtModified { get; set; }
    public bool IsActive { get; set; }
}
