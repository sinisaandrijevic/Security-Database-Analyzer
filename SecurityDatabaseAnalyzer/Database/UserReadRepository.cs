using Microsoft.Data.Sqlite;
using SecurityDatabaseAnalyzer.Models;

namespace SecurityDatabaseAnalyzer.Database;

public static class UserReadRepository
{
    public static List<UserRecord> LoadUsers(SqliteConnection connection)
    {
        var users = new List<UserRecord>();

        const string sql = """
                           SELECT
                               id,
                               username,
                               failed_attempts,
                               locked,
                               created_at
                           FROM users
                           ORDER BY username;
                           """;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserRecord
            {
                Id = reader.GetInt32(0),
                Username = reader.GetString(1),
                FailedAttempts = reader.GetInt32(2),
                Locked = reader.GetInt32(3) == 1,
                CreatedAt = reader.GetString(4)
            });
        }

        return users;
    }
}