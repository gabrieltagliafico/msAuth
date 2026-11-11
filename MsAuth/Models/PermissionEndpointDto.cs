namespace BNC.Microservices.Auth.Models
{
    public record PermissionEndpointDto
    {
        public int ResultCode { get; set; }
        public int IdUser { get; set; }
        public Guid UidUser { get; set; }
        public int IdService { get; set; }
        public int? IdEndpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? Route { get; set; }
        public string? Name { get; set; }
    }
}
