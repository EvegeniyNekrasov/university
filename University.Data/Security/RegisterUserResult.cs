namespace University.Data.Security;

public enum RegisterFailureReason
{
    None = 0,
    DuplicateEmail = 1,
    InvalidData = 2
}

public sealed record RegisterUserResult(
    bool Succeeded,
    RegisterFailureReason FailureReason,
    Guid? UserId,
    string? Email,
    string? DisplayName,
    string? SecurityStamp,
    IReadOnlyList<string> Roles
)
{
    public static RegisterUserResult Duplicate() =>
        new(false, RegisterFailureReason.DuplicateEmail, null, null, null, null, Array.Empty<string>());

    public static RegisterUserResult Invalid() =>
        new(false, RegisterFailureReason.InvalidData, null, null, null, null, Array.Empty<string>());

    public static RegisterUserResult Success(
        Guid userId,
        string email,
        string? displayName,
        string securityStamp,
        IReadOnlyList<string> roles) =>
        new(true, RegisterFailureReason.None, userId, email, displayName, securityStamp, roles);
}