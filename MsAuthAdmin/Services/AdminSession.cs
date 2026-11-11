using System.Globalization;

namespace MsAuthAdmin.Services;

public static class AdminSession
{
    private const string TokenKey = "msauth.token";
    private const string ExpiresKey = "msauth.expires";
    private const string UserKey = "msauth.user";

    public static void SignIn(ISession session, string token, DateTime expires, Guid userId)
    {
        var utc = expires.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(expires, DateTimeKind.Utc)
            : expires.ToUniversalTime();

        session.SetString(TokenKey, token);
        session.SetString(ExpiresKey, utc.ToString("O", CultureInfo.InvariantCulture));
        session.SetString(UserKey, userId.ToString());
    }

    public static void SignOut(ISession session)
    {
        session.Clear();
    }

    public static bool IsActive(ISession session)
    {
        return TryGetToken(session, out _);
    }

    public static bool TryGetToken(ISession session, out string token)
    {
        token = session.GetString(TokenKey) ?? string.Empty;
        var rawExpires = session.GetString(ExpiresKey);
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(rawExpires))
        {
            return false;
        }

        if (!DateTime.TryParse(rawExpires, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expires))
        {
            return false;
        }

        return expires > DateTime.UtcNow;
    }

    public static DateTime? GetExpires(ISession session)
    {
        var rawExpires = session.GetString(ExpiresKey);
        if (string.IsNullOrEmpty(rawExpires))
        {
            return null;
        }

        return DateTime.TryParse(rawExpires, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expires)
            ? expires
            : null;
    }

    public static string? UserId(ISession session)
    {
        return session.GetString(UserKey);
    }
}
