namespace University.Data.Security;

public interface IAuthService
{
    Task<AuthSignInResult> PasswordSignInAsync(string email, string password, CancellationToken ct);

    Task CreateUserAsync(
        string email,
        string password,
        string? displayName,
        bool emailConfirmed,
        CancellationToken ct);

    Task<RegisterUserResult> RegisterUserAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken ct);
}
