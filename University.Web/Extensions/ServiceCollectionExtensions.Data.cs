using University.Data.Configuration;

namespace University.Web.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppData(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AppPostgresDataAccess(configuration);

        return services;
    }
}
