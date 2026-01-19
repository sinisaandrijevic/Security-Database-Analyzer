using Microsoft.Data.Sqlite;

namespace SecurityDatabaseAnalyzer.Database;


/// Servis za otvaranje SQLite baze samo u READ-ONLY režimu.
public static class SQLiteReadOnlyService
{
    public static SqliteConnection Open(string databasePath, bool readOnly = true)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();

        return connection;
    }
}