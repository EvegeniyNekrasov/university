namespace University.Data.Auth;

public interface IAuthRepository
{
    Task<AuthUserRow?> FindByEmailAsync(
        string email,
        CancellationToken ct);
    Task<IReadOnlyList<string>> GetRolesAsync(
        Guid userId,
        CancellationToken ct);
    Task<AuthValidationRow?> GetValidationStateAsync(
        Guid userId,
        CancellationToken ct);

    Task CreateUserAsync(
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
