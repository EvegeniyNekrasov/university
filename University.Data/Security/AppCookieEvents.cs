using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using University.Data.Auth;


namespace University.Data.Security;

public sealed class AppCookieEvents(IAuthRepository authRepository) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userIdText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var cookieStamp = context.Principal?.FindFirstValue("security_stamp");

        if (!Guid.TryParse(userIdText, out var userId) || string.IsNullOrWhiteSpace(cookieStamp))
        {
            await RejectAsync(context);
            return;
        }

        var state = await authRepository.GetValidationStateAsync(userId, context.HttpContext.RequestAborted);

        if (state is null || !state.IsActive || !string.Equals(state.SecurityStamp, cookieStamp, StringComparison.Ordinal))
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}