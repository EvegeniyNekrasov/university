using Microsoft.AspNetCore.RateLimiting;

namespace University.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("login", limiterOptions =>
            {
                limiterOptions.PermitLimit = 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("register", limiterOptions =>
            {
                limiterOptions.PermitLimit = 3;
                limiterOptions.Window = TimeSpan.FromMinutes(5);
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }
}
