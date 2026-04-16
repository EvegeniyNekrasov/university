using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace University.Data.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AppPostgresDataAccess(
        this IServiceCollection services,
        IConfiguration configuration
        )
    {
        services.AddOptions<PostgresOptions>()
          .Bind(configuration.GetSection(PostgresOptions.SectionName))
          .ValidateDataAnnotations()
          .Validate(o => o.MaxPoolSize >= o.MinPoolSize, "Postgres: MaxPoolSize debe ser >= MinPoolSize")
          .ValidateOnStart();

        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = options.Host,
                Port = options.Port,
                Database = options.Database,
                Username = options.Username,
                Password = options.Password,

                Pooling = true,
                MinPoolSize = options.MinPoolSize,
                MaxPoolSize = options.MaxPoolSize,
                Timeout = options.TimeoutSeconds,
                CommandTimeout = options.CommandTimeoutSeconds,
                KeepAlive = options.KeepAliveSeconds,
                ApplicationName = options.ApplicationName,

                SslMode = options.SslMode,
                RootCertificate = options.RootCertificatePath,

                IncludeErrorDetail = false
            };

            var builder = new NpgsqlDataSourceBuilder(csb.ConnectionString);
            builder.UseLoggerFactory(loggerFactory);

            return builder.Build();
        });

        #region DAO
        #endregion

        #region REPOSITORY 
        #endregion

        return services;
    }
}
