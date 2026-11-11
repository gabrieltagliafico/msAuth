namespace BNC.Microservices.Auth.Models
{
    public record IssueTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Bearer { get; set; } = string.Empty;
        public DateTime DtExpires { get; set; }
    }
}
