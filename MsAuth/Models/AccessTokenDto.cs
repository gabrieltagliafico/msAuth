namespace BNC.Microservices.Auth.Models
{
    public record AccessTokenDto
    {
        public int IdAccessToken { get; set; }
        public DateTime DtCreated { get; set; }
        public DateTime DtExpires { get; set; }
    }
}
