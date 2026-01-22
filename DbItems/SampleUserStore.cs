using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;

namespace SampleApp.DbItems
{
    public class SampleUserStore :
        IUserPasswordStore<SampleUser>
    {

        private readonly string _connectionString;

        public SampleUserStore(string connectionString)
        {
            _connectionString = connectionString;
            // Ensure the database and Users table exist
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    UserName TEXT,
                    NormalizedUserName TEXT,
                    PasswordHash TEXT,
                    DisplayName TEXT,
                    ConcurrencyStamp TEXT,
                    SecurityStamp TEXT
                )";
            command.ExecuteNonQuery();

            connection.Close();
        }

        private static void FillParams(SqliteCommand command, SampleUser user)
        {
            command.Parameters.AddRange(new[] {
                    new SqliteParameter( "$id", user.Id.ToString() ),
                    new SqliteParameter( "$userName", user.UserName ?? string.Empty ),
                    new SqliteParameter( "$normalizedUserName", user.NormalizedUserName ?? string.Empty ),
                    new SqliteParameter( "$passwordHash", user.PasswordHash ?? string.Empty ),
                    new SqliteParameter( "$displayName", user.DisplayName ?? string.Empty ),
                    new SqliteParameter( "$concurrencyStamp", user.ConcurrencyStamp ?? string.Empty ),
                    new SqliteParameter( "$securityStamp", user.SecurityStamp ?? string.Empty )
            });
        }

        public async Task<IdentityResult> CreateAsync(SampleUser user, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO Users (Id, UserName, NormalizedUserName, PasswordHash, DisplayName, ConcurrencyStamp, SecurityStamp)
                VALUES ($id, $userName, $normalizedUserName, $passwordHash, $displayName, $concurrencyStamp, $securityStamp)";
                FillParams(command, user);
                await command.ExecuteNonQueryAsync(cancellationToken);
                return IdentityResult.Success;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }



        public async Task<IdentityResult> DeleteAsync(SampleUser user, CancellationToken cancellationToken)
        {
            using SqliteConnection _connection = new SqliteConnection(_connectionString);
            await _connection.OpenAsync(cancellationToken);
            using SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                DELETE FROM Users
                WHERE Id = $id";
            command.Parameters.AddWithValue("$id", user.Id.ToString());
            await command.ExecuteNonQueryAsync(cancellationToken);
            await _connection.CloseAsync();
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }


        public async Task<SampleUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Id, UserName, NormalizedUserName, PasswordHash, DisplayName, ConcurrencyStamp, SecurityStamp
                FROM Users
                WHERE Id = $id";
                command.Parameters.AddWithValue("$id", userId);
                using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    SampleUser user = new SampleUser
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        UserName = reader.GetString(1),
                        NormalizedUserName = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        DisplayName = reader.GetString(4)
                    };
                    return user;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return null;
        }

        public async Task<SampleUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Id, UserName, NormalizedUserName, PasswordHash, DisplayName, ConcurrencyStamp, SecurityStamp
                FROM Users
                WHERE NormalizedUserName = $normalizedUserName";
                command.Parameters.AddWithValue("$normalizedUserName", normalizedUserName);
                using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    SampleUser user = new SampleUser
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        UserName = reader.GetString(1),
                        NormalizedUserName = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        DisplayName = reader.GetString(4),
                        ConcurrencyStamp = reader.GetString(5),
                        SecurityStamp = reader.GetString(6)
                    };
                    return user;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return null;

        }

        public Task<string?> GetNormalizedUserNameAsync(SampleUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string?> GetPasswordHashAsync(SampleUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(SampleUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string?> GetUserNameAsync(SampleUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task<bool> HasPasswordAsync(SampleUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetNormalizedUserNameAsync(SampleUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(SampleUser user, string? passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(SampleUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(SampleUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Users
                    SET UserName = $userName,
                        NormalizedUserName = $normalizedUserName,
                        PasswordHash = $passwordHash,
                        DisplayName = $displayName,
                        ConcurrencyStamp = $concurrencyStamp,
                        SecurityStamp = $securityStamp
                    WHERE Id = $id";
                FillParams(command, user);
                int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                if (rowsAffected > 0)
                {
                    return IdentityResult.Success;
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return IdentityResult.Failed();
        }

        public async Task<IDictionary<Guid, SampleUser>> GetUserListAsync(IList<Guid> ids, CancellationToken cancellationToken)
        {
            Dictionary<Guid, SampleUser> users = new Dictionary<Guid, SampleUser>();
            using SqliteConnection connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = @"
                SELECT Id, UserName, NormalizedUserName, PasswordHash, DisplayName, ConcurrencyStamp, SecurityStamp
                FROM Users
                WHERE Id IN (" + string.Join(",", ids.Select((_, index) => $"$id{index}")) + ")";
                for (int i = 0; i < ids.Count; i++)
                {
                    command.Parameters.AddWithValue($"$id{i}", ids[i].ToString());
                }
                using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    SampleUser user = new SampleUser
                    {
                        Id = Guid.Parse(reader.GetString(0)),
                        UserName = reader.GetString(1),
                        NormalizedUserName = reader.GetString(2),
                        PasswordHash = reader.GetString(3),
                        DisplayName = reader.GetString(4),
                        ConcurrencyStamp = reader.GetString(5),
                        SecurityStamp = reader.GetString(6)
                    };
                    users.Add(user.Id, user);
                }
            }
            finally
            {
                await connection.CloseAsync();
            }
            return users;
        }



    }
}
