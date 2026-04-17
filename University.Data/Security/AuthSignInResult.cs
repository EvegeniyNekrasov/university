namespace University.Data.Security;

public enum AuthFailureReason
{
    None = 0,
    InvalidCredentials = 1,
    LockedOut = 2
}

public sealed record AuthSignInResult(
    bool Succeeded,
    AuthFailureReason FailureReason,
    Guid? UserId,
    string? Email,
    string? DisplayName,
    string? SecurityStamp,
    IReadOnlyList<string> Roles)
{
    public static AuthSignInResult Invalid() =>
        new(false, AuthFailureReason.InvalidCredentials, null, null, null, null, []);

    public static AuthSignInResult LockedOut() =>
        new(false, AuthFailureReason.LockedOut, null, null, null, null, []);

    public static AuthSignInResult Success(
        Guid userId,
        string email,
        string? displayName,
        string securityStamp,
        IReadOnlyList<string> roles) =>
        new(true, AuthFailureReason.None, userId, email, displayName, securityStamp, roles);
}