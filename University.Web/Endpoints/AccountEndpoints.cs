using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using University.Data.Security;
using University.Web.Utils.Auth;

namespace University.Web.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/account/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting("login");

        endpoints.MapPost("/account/register", RegisterAsync)
            .AllowAnonymous()
            .RequireRateLimiting("register");

        endpoints.MapPost("/account/logout", (Delegate)LogoutAsync)
            .RequireAuthorization();

        endpoints.MapGet("/account/logout", () => Results.Redirect("/login"))
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        [FromForm] LoginPostModel model,
        HttpContext httpContext,
        IAuthService authService,
        IOptions<AuthOptions> authOptionsAccessor,
        CancellationToken ct)
    {
        var returnUrl = AuthHelper.IsSafeLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/";

        var result = await authService.PasswordSignInAsync(model.Email, model.Password, ct);

        if (!result.Succeeded)
        {
            var error = result.FailureReason == AuthFailureReason.LockedOut
                ? "locked"
                : "invalid";

            return Results.Redirect($"/login?error={error}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var principal = BuildPrincipal(
            result.UserId!.Value,
            result.Email!,
            result.DisplayName,
            result.SecurityStamp!,
            result.Roles);

        var properties = new AuthenticationProperties();

        if (model.RememberMe)
        {
            properties.IsPersistent = true;
            properties.ExpiresUtc = DateTimeOffset.UtcNow
                .AddDays(authOptionsAccessor.Value.PersistentSessionDays);
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        return SeeOther(returnUrl);
    }

    private static async Task<IResult> RegisterAsync(
        [FromForm] RegisterPostModel model,
        HttpContext httpContext,
        IAuthService authService,
        CancellationToken ct)
    {
        var returnUrl = AuthHelper.IsSafeLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/";

        if (string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password) ||
            model.Password != model.ConfirmPassword)
        {
            return Results.Redirect($"/register?error=invalid&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var result = await authService.RegisterUserAsync(
            model.Email,
            model.Password,
            model.DisplayName,
            ct);

        if (!result.Succeeded)
        {
            var error = result.FailureReason switch
            {
                RegisterFailureReason.DuplicateEmail => "duplicate",
                _ => "invalid"
            };

            return Results.Redirect($"/register?error={error}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var principal = BuildPrincipal(
            result.UserId!.Value,
            result.Email!,
            result.DisplayName,
            result.SecurityStamp!,
            result.Roles);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        return SeeOther(returnUrl);
    }


    private static async Task<IResult> LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login?ReturnUrl=%2Faccount%2Flogout");
    }

    private static ClaimsPrincipal BuildPrincipal(
        Guid userId,
        string email,
        string? displayName,
        string securityStamp,
        IReadOnlyList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, displayName ?? email),
            new(ClaimTypes.Email, email),
            new("security_stamp", securityStamp)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    private static SeeOtherResult SeeOther(string location) => new(location);
}