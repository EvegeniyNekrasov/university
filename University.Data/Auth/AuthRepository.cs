
namespace University.Data.Auth;

public sealed class AuthRepository(IAuthUserDao dao) : IAuthRepository
{
    public Task<AuthUserRow?> FindByEmailAsync(string email, CancellationToken ct)
        => dao.GetByNormalizedEmailAsync(NormalizeEmail(email), ct);

    public Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct)
        => dao.GetRoleNamesAsync(userId, ct);

    public Task<AuthValidationRow?> GetValidationStateAsync(Guid userId, CancellationToken ct)
        => dao.GetValidationRowAsync(userId, ct);

    public Task CreateUserAsync(CreateAuthuserRow user, CancellationToken ct)
        => dao.InsertUserAsync(user, ct);

    public Task AssignRoleAsync(Guid userId, string roleName, CancellationToken ct)
        => dao.AssignRoleAsync(userId, roleName, ct);

    public Task RegisterFailedLoginAsync(Guid userId, int maxFailedAttempts, TimeSpan lockoutDuration, CancellationToken ct)
        => dao.RegisterFailedLoginAsync(userId, maxFailedAttempts, lockoutDuration, ct);

    public Task RegisterSuccessfulLoginAsync(Guid userId, CancellationToken ct)
        => dao.RegisterSuccessfulLoginAsync(userId, ct);

    public Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken ct)
        => dao.UpdatePasswordHashAsync(userId, newPasswordHash, ct);

    public Task RotateSecurityStampAsync(Guid userId, string newSecurityStamp, CancellationToken ct)
        => dao.RotateSecurityStampAsync(userId, newSecurityStamp, ct);

    private static string NormalizeEmail(string email)
        => email.Trim().ToUpperInvariant();
}