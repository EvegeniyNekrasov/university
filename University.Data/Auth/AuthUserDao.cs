using Npgsql;

namespace University.Data.Auth;

public sealed class AuthUserDao(NpgsqlDataSource dataSource) : IAuthUserDao
{
    public async Task<AuthUserRow?> GetByNormalizedEmailAsync
    (
        string normalizedEmail,
        CancellationToken ct
    )
    {
        const string sql = """
            select
                id,
                email,
                normalized_email,
                password_hash,
                display_name,
                is_active,
                email_confirmed,
                access_failed_count,
                lockout_end_utc,
                security_stamp
            from auth.users
            where normalized_email = @normalized_email
            limit 1;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("normalized_email", normalizedEmail);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new AuthUserRow(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetBoolean(5),
            reader.GetBoolean(6),
            reader.GetInt32(7),
            reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTime>(8),
            reader.GetString(9));
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            select r.name
            from auth.user_roles ur
            inner join auth.roles r on r.id = ur.role_id
            where ur.user_id = @user_id
            order by r.name;
            """;

        var roles = new List<string>();

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            roles.Add(reader.GetString(0));
        }

        return roles;
    }

    public async Task<AuthValidationRow?> GetValidationRowAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            select id, is_active, security_stamp
            from auth.users
            where id = @user_id
            limit 1;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
            return null;

        return new AuthValidationRow(
            reader.GetGuid(0),
            reader.GetBoolean(1),
            reader.GetString(2));
    }

    public async Task InsertUserAsync(CreateAuthuserRow user, CancellationToken ct)
    {
        const string sql = """
            insert into auth.users
            (
                id,
                email,
                normalized_email,
                password_hash,
                display_name,
                is_active,
                email_confirmed,
                security_stamp
            )
            values
            (
                @id,
                @email,
                @normalized_email,
                @password_hash,
                @display_name,
                @is_active,
                @email_confirmed,
                @security_stamp
            );
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("id", user.Id);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("normalized_email", user.NormalizedEmail);
        cmd.Parameters.AddWithValue("password_hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("display_name", (object?)user.DisplayName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("is_active", user.IsActive);
        cmd.Parameters.AddWithValue("email_confirmed", user.EmailConfirmed);
        cmd.Parameters.AddWithValue("security_stamp", user.SecurityStamp);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RegisterFailedLoginAsync(
        Guid userId,
        int maxFailedAttempts,
        TimeSpan lockoutDuration,
        CancellationToken ct)
    {
        const string sql = """
            update auth.users
            set
                access_failed_count = access_failed_count + 1,
                lockout_end_utc = case
                    when access_failed_count + 1 >= @max_failed_attempts
                        then now() + @lockout_duration
                    else lockout_end_utc
                end,
                updated_utc = now()
            where id = @user_id;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("max_failed_attempts", maxFailedAttempts);
        cmd.Parameters.AddWithValue("lockout_duration", lockoutDuration);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RegisterSuccessfulLoginAsync(Guid userId, CancellationToken ct)
    {
        const string sql = """
            update auth.users
            set
                access_failed_count = 0,
                lockout_end_utc = null,
                last_login_utc = now(),
                updated_utc = now()
            where id = @user_id;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("user_id", userId);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string newPasswordHash, CancellationToken ct)
    {
        const string sql = """
            update auth.users
            set
                password_hash = @password_hash,
                updated_utc = now()
            where id = @user_id;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("password_hash", newPasswordHash);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task RotateSecurityStampAsync(Guid userId, string newSecurityStamp, CancellationToken ct)
    {
        const string sql = """
            update auth.users
            set
                security_stamp = @security_stamp,
                updated_utc = now()
            where id = @user_id;
            """;

        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("security_stamp", newSecurityStamp);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}