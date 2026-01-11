using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using SecurityDatabaseAnalyzer.Models;

namespace SecurityDatabaseAnalyzer.Database;

public static class LoginEventReadRepository
{
    public static List<LoginEventRecord> LoadLoginEvents(SqliteConnection connection)
    {
        var result = new List<LoginEventRecord>();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
                              SELECT
                                  username,
                                  success,
                                  mode,
                                  reason,
                                  occurred_at
                              FROM login_events
                              ORDER BY occurred_at DESC
                          """;

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            result.Add(new LoginEventRecord
            {
                Username = reader.GetString(0),
                Success = reader.GetInt32(1) == 1,
                Mode = reader.GetString(2),
                Reason = reader.IsDBNull(3) ? "" : reader.GetString(3),
                OccurredAt = DateTime.Parse(reader.GetString(4))
            });
        }

        return result;
    }
}