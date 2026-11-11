namespace BNC.Microservices.Auth.Models
{
    public record IssueTokenResult
    {
        public bool IsAuthorized { get; set; }
        public IssueTokenResponse? Token { get; set; }
    }
}
