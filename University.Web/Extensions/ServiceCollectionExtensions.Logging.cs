using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace University.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "UniversityApp")
            .Enrich.WithProperty("Environment", environment.EnvironmentName));

        return services;
    }
}
