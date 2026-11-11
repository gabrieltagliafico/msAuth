namespace BNC.Microservices.Auth
{
    public class RepositorySettings
    {
        public BncAuth BNCAuth { get; set; } = new();
    }

    public class BncAuth
    {
        public Dictionary<string, string> StoredProcedures { get; set; } = new();
    }
}
