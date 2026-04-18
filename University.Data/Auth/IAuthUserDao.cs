namespace University.Data.Auth;

public interface IAuthUserDao
{
    Task<AuthUserRow?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken ct);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(
        Guid userId,
        CancellationToken ct);
    Task<AuthValidationRow?> GetValidationRowAsync(
        Guid userId,
        CancellationToken ct);

    Task InsertUserAsync(
        CreateAuthuserRow user,
        CancellationToken ct);

    Task AssignRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken ct);

    Task RegisterFailedLoginAsync(
        Guid userId,
        int maxFailedAttempts,
        TimeSpan lockoutDuration,
        CancellationToken ct);

    Task RegisterSuccessfulLoginAsync(
        Guid userId,
        CancellationToken ct);

    Task UpdatePasswordHashAsync(
        Guid userId,
        string newPasswordHash,
        CancellationToken ct);

    Task RotateSecurityStampAsync(
        Guid userId,
        string newSecurityStamp,
        CancellationToken ct);
}
