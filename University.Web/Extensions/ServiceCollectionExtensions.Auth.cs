using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using University.Data.Security;

namespace University.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        const string _applicationName = "University";
        const string _cookieName = "University.Auth";

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IPasswordHasher<AuthPasswordUser>, PasswordHasher<AuthPasswordUser>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<AppCookieEvents>();

        services.AddDataProtection()
            .SetApplicationName(_applicationName);

        var authOptions = configuration
            .GetSection(AuthOptions.SectionName)
            .Get<AuthOptions>() ?? new AuthOptions();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/forbidden";

                options.Cookie.Name = _cookieName;
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(authOptions.SessionMinutes);
                options.SlidingExpiration = true;

                options.EventsType = typeof(AppCookieEvents);
            });

        return services;
    }
}
