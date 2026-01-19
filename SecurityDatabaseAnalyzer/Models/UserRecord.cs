namespace SecurityDatabaseAnalyzer.Models;

public class UserRecord
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public int FailedAttempts { get; init; }
    public bool Locked { get; init; }
    public string CreatedAt { get; init; } = string.Empty;
    public bool IsHighRiskUser { get; set; }
}