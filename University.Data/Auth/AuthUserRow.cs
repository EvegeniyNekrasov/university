namespace University.Data.Auth;

public sealed record AuthUserRow(
    Guid Id,
    string Email,
    string NormalizedEmail,
    string PasswordHash,
    string? DisplayName,
    bool IsActive,
    bool EmailConfirmed,
    int AccessFailedCount,
    DateTime? LockoutEndUtc,
    string SecurityStamp
);

public sealed record AuthValidationRow
(
    Guid Id,
    bool IsActive,
    string SecurityStamp
);

public sealed record CreateAuthuserRow
(
    Guid Id,
    string Email,
    string NormalizedEmail,
    string PasswordHash,
    string? DisplayName,
    bool IsActive,
    bool EmailConfirmed,
    string SecurityStamp
);