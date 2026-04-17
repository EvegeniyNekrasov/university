using Serilog;
using University.Web.Components;
using University.Web.Endpoints;
using University.Web.Extensions;
using University.Web.Infrastructure.Logging;
using University.Web.Infrastructure.Seed;

LoggingSetup.ConfigureBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Services
        .AddAppUi()
        .AddAppData(builder.Configuration)
        .AddAppAuth(builder.Configuration)
        .AddAppAuthorization()
        .AddAppRateLimiting()
        .AddAppLogging(builder.Configuration, builder.Environment);

    var app = builder.Build();

    app.ConfigureRequestLogging();
    app.UseAppPipeline();

    if (app.Environment.IsDevelopment())
    {
        await DevelopmentDataSeeder.SeedAsync(app.Services);
    }

    // Esto es para que funcione MudBlazor.css en la página Login con EmptyLayout
    app.MapStaticAssets().AllowAnonymous();

    app.MapAccountEndpoints();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicación paró inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}