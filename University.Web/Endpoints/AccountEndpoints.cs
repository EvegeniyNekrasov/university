using System;
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

        endpoints.MapPost("/account/logout", LogoutAsync)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        [FromForm] LoginPostModel model,
        HttpContext httpContext,
        IAuthService authService,
        IOptions<AuthOptions> authOptionsAccessor,
        CancellationToken ct)
    {
        var returnUrl = AuthHelper.IsSafeLocalUrl(model.ReturnUrl)
            ? model.ReturnUrl!
            : "/";

        var result = await authService.PasswordSignInAsync(model.Email, model.Password, ct);

        if (!result.Succeeded)
        {
            var error = result.FailureReason == AuthFailureReason.LockedOut
                ? "locked"
                : "invalid";

            return Results.Redirect($"/login?error={error}&returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
            new(ClaimTypes.Name, result.DisplayName ?? result.Email!),
            new(ClaimTypes.Email, result.Email!),
            new("security_stamp", result.SecurityStamp!)
        };

        foreach (var role in result.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties();

        if (model.RememberMe)
        {
            properties.IsPersistent = true;
            properties.ExpiresUtc = DateTimeOffset.UtcNow
                .AddDays(authOptionsAccessor.Value.PersistentSessionDays);
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);

        return Results.Redirect(returnUrl);
    }

    private static async Task<IResult> LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login");
    }
}