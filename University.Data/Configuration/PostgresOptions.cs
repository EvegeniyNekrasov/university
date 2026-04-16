using System.ComponentModel.DataAnnotations;
using Npgsql;

namespace University.Data.Configuration;

public sealed class PostgresOptions
{
    public const string SectionName = "Postgres";

    public required string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 5432;

    public required string Database { get; init; } = string.Empty;

    public required string Username { get; init; } = string.Empty;

    public required string Password { get; init; } = string.Empty;

    public string? RootCertificatePath { get; init; }

    [Range(0, 500)]
    public int MinPoolSize { get; init; } = 2;

    [Range(0, 5000)]
    public int MaxPoolSize { get; init; } = 50;

    [Range(1, 30)]
    public int TimeoutSeconds { get; init; } = 5;

    [Range(1, 300)]
    public int CommandTimeoutSeconds { get; init; } = 30;

    [Range(0, 3600)]
    public int KeepAliveSeconds { get; init; } = 30;

    public required string ApplicationName { get; init; } = "MyApp";

    public SslMode SslMode { get; init; } = SslMode.VerifyFull;
}
