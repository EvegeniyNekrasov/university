namespace University.Data.Security;

public sealed record AuthPasswordUser(
    Guid UserId,
    string Email,
    string SecurityStamp);
