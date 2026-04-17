using University.Data.Auth;
using University.Data.Security;

namespace University.Web.Infrastructure.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();

        var admin = await authRepository.FindByEmailAsync("admin@local.test", ct);
        if (admin is null)
        {
            await authService.CreateUserAsync(
                email: "admin@local.test",
                password: "Admin1234!Cambiar",
                displayName: "Administrador",
                emailConfirmed: true,
                ct: ct);
        }

        var user = await authRepository.FindByEmailAsync("user@local.test", ct);
        if (user is null)
        {
            await authService.CreateUserAsync(
                email: "user@local.test",
                password: "User1234!Cambiar",
                displayName: "Usuario Demo",
                emailConfirmed: true,
                ct: ct);
        }
    }
}
