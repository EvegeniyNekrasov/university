using Microsoft.AspNetCore.Authorization;

namespace University.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        return services;
    }
}
