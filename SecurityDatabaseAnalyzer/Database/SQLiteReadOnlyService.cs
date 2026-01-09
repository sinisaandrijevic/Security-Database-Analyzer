using Microsoft.Data.Sqlite;

namespace SecurityDatabaseAnalyzer.Database;


/// Servis za otvaranje SQLite baze samo u READ-ONLY režimu.
public static class SQLiteReadOnlyService
{
    public static SqliteConnection Open(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        return connection;
    }
}