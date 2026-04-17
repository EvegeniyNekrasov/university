using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using University.Data.Auth;

namespace University.Data.Security;

public sealed class AuthService(
    IAuthRepository authRepository,
    IPasswordHasher<AuthPasswordUser> passwordHasher,
    IOptions<AuthOptions> authOptions) : IAuthService
{
    public async Task<AuthSignInResult> PasswordSignInAsync(string email, string password, CancellationToken ct)
    {
        var user = await authRepository.FindByEmailAsync(email, ct);

        if (user is null || !user.IsActive)
            return AuthSignInResult.Invalid();

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc > DateTime.UtcNow)
            return AuthSignInResult.LockedOut();

        var passwordUser = new AuthPasswordUser(user.Id, user.Email, user.SecurityStamp);

        var verifyResult = passwordHasher.VerifyHashedPassword(
            passwordUser,
            user.PasswordHash,
            password);

        if (verifyResult == PasswordVerificationResult.Failed)
        {
            var options = authOptions.Value;

            await authRepository.RegisterFailedLoginAsync(
                user.Id,
                options.MaxFailedAccessAttempts,
                TimeSpan.FromMinutes(options.LockoutMinutes),
                ct);

            return AuthSignInResult.Invalid();
        }

        if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newHash = passwordHasher.HashPassword(passwordUser, password);
            await authRepository.UpdatePasswordHashAsync(user.Id, newHash, ct);
        }

        await authRepository.RegisterSuccessfulLoginAsync(user.Id, ct);
        var roles = await authRepository.GetRolesAsync(user.Id, ct);

        return AuthSignInResult.Success(
            user.Id,
            user.Email,
            user.DisplayName,
            user.SecurityStamp,
            roles);
    }

    public async Task CreateUserAsync(
        string email,
        string password,
        string? displayName,
        bool emailConfirmed,
        CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var userId = Guid.NewGuid();
        var securityStamp = Guid.NewGuid().ToString("N");

        var passwordUser = new AuthPasswordUser(userId, email, securityStamp);
        var passwordHash = passwordHasher.HashPassword(passwordUser, password);

        var row = new CreateAuthuserRow(
            userId,
            email.Trim(),
            normalizedEmail,
            passwordHash,
            displayName,
            IsActive: true,
            EmailConfirmed: emailConfirmed,
            SecurityStamp: securityStamp);

        await authRepository.CreateUserAsync(row, ct);
    }
}