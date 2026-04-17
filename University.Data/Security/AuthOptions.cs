using System.ComponentModel.DataAnnotations;

namespace University.Data.Security;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    [Range(1, 20)]
    public int MaxFailedAccessAttempts { get; init; } = 5;

    [Range(1, 1440)]
    public int LockoutMinutes { get; init; } = 15;

    [Range(5, 1440)]
    public int SessionMinutes { get; init; } = 20;

    [Range(1, 90)]
    public int PersistentSessionDays { get; init; } = 14;
}
