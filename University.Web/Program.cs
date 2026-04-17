using University.Data.Configuration;
using University.Web.Components;
using Serilog;
using Serilog.Events;
using MudBlazor.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using University.Data.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using University.Web.Utils.Auth;
using Microsoft.Extensions.Options;
using University.Data.Auth;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddMudServices();
    builder.Services.AppPostgresDataAccess(builder.Configuration);

    builder.Services.AddOptions<AuthOptions>()
        .Bind(builder.Configuration.GetSection(AuthOptions.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();

    builder.Services.AddScoped<IPasswordHasher<AuthPasswordUser>, PasswordHasher<AuthPasswordUser>>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<AppCookieEvents>();

    builder.Services.AddDataProtection()
        .SetApplicationName("University");

    var authOptions = builder.Configuration
        .GetSection(AuthOptions.SectionName)
        .Get<AuthOptions>() ?? new AuthOptions();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/forbidden";

        options.Cookie.Name = "University.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.ExpireTimeSpan = TimeSpan.FromMinutes(authOptions.SessionMinutes);
        options.SlidingExpiration = true;

        options.EventsType = typeof(AppCookieEvents);
    });

    builder.Services.AddAuthorizationBuilder()
        .SetFallbackPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build())
        .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueLimit = 0;
        });
    });

    builder.Services.AddSerilog((services, ls) => ls
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "UniversityApp")
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName));


    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (ex != null || httpContext.Response.StatusCode >= 500)
            {
                return LogEventLevel.Error;
            }

            if (httpContext.Response.StatusCode >= 400)
            {
                return LogEventLevel.Warning;
            }

            return LogEventLevel.Information;
        };
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name!);
            }

        };
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();

    }
    else
    {
        using (var scope = app.Services.CreateScope())
        {
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var authRepository = scope.ServiceProvider.GetRequiredService<IAuthRepository>();
            var ct = CancellationToken.None;

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
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
    app.UseRateLimiter();

    // Esto es para que funciones el  MudBlazor.css en la página Login con el EmptyLayout
    app.MapStaticAssets().AllowAnonymous();

    app.MapPost("/account/login", async (
        [FromForm] LoginPostModel model,
        HttpContext httpContext,
        IAuthService authService,
        IOptions<AuthOptions> authOptionsAccessor,
        CancellationToken ct) =>
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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId!.Value.ToString()),
            new(ClaimTypes.Name, result.DisplayName ?? result.Email!),
            new(ClaimTypes.Email, result.Email!),
            new("security_stamp", result.SecurityStamp!)
        };

        foreach (var role in result.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties();

        if (model.RememberMe)
        {
            properties.IsPersistent = true;
            properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(authOptionsAccessor.Value.PersistentSessionDays);
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            properties);

        return Results.Redirect(returnUrl);
    })
    .AllowAnonymous()
    .RequireRateLimiting("login");

    app.MapPost("/account/logout", async (HttpContext httpContext) =>
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Redirect("/login");
    })
    .RequireAuthorization();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Applicación paro inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}






