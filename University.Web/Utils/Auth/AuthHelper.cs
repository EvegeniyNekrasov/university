namespace University.Web.Utils.Auth;

public static class AuthHelper
{
    public static bool IsSafeLocalUrl(string? url) =>
        !string.IsNullOrWhiteSpace(url) &&
        url.StartsWith('/') &&
        !url.StartsWith("//") &&
        !url.StartsWith("/\\");
}
