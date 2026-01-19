namespace SecurityDatabaseAnalyzer.Models;

public class LoginEventRecord
{
    public string Username { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Mode { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public bool IsHighRisk { get; init; }
}