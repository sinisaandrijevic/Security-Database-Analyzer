using Microsoft.Win32;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using System.Windows;
using SecurityDatabaseAnalyzer.Database;
using SecurityDatabaseAnalyzer.Models;

namespace SecurityDatabaseAnalyzer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

using System.Collections.Generic;

public partial class MainWindow : Window
{
    private List<UserRecord> _allUsers = new();
    private List<LoginEventRecord> _allLoginEvents = new();
    private string _lastDatabasePath = null;

    // Helper for suspicious login event detection
    private bool IsSuspiciousLoginEvent(LoginEventRecord ev)
    {
        // If username does not exist in _allUsers, it's highly suspicious
        bool userExists = _allUsers.Any(u => u.Username.Equals(ev.Username, StringComparison.OrdinalIgnoreCase));
        if (!userExists)
            return true;

        // Check for weird strings (basic SQLi patterns)
        if (!string.IsNullOrEmpty(ev.Reason))
        {
            string reason = ev.Reason.ToLower();
            if (reason.Contains("sql_injection_possible") || reason.Contains("' or 1=1") || reason.Contains("--") || reason.Contains("/*") || reason.Contains("union select"))
                return true;
        }
        return false;
    }

    private void UnlockUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is UserRecord user)
        {
            if (user.Locked && _lastDatabasePath != null)
            {
                try
                {
                    using var connection = SQLiteReadOnlyService.Open(_lastDatabasePath, readOnly: false);
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = "UPDATE users SET locked = 0 WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    int affected = cmd.ExecuteNonQuery();
                    if (affected > 0)
                    {
                        // Update in-memory
                        var unlockedUser = new UserRecord
                        {
                            Id = user.Id,
                            Username = user.Username,
                            FailedAttempts = user.FailedAttempts,
                            Locked = false,
                            CreatedAt = user.CreatedAt
                        };
                        int idx = _allUsers.FindIndex(u => u.Id == user.Id);
                        if (idx >= 0)
                        {
                            _allUsers[idx] = unlockedUser;
                            UsersDataGrid.ItemsSource = null;
                            UsersDataGrid.ItemsSource = _allUsers;
                        }
                        MessageBox.Show($"User '{user.Username}' unlocked and saved to database.", "Unlock User", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to unlock user '{user.Username}' in database.", "Unlock User", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error unlocking user: {ex.Message}", "Unlock User", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
    }

    // Poziva se klikom na dugme "Open Database"
    private void OpenDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select authentication database",
            Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            _lastDatabasePath = dialog.FileName;
            // 1. Otvori bazu ISKLJUČIVO u read-only režimu
            using var connection = SQLiteReadOnlyService.Open(_lastDatabasePath);

            // ===================== USERS =====================
            var users = UserReadRepository.LoadUsers(connection);
            _allUsers = users.ToList();
            UsersDataGrid.ItemsSource = _allUsers;

            // ===================== SECURITY INSIGHTS =====================
            TotalUsersText.Text = $"Total users: {users.Count}";

            var lockedUsers = users.Count(u => u.Locked);
            LockedUsersText.Text = $"Locked users: {lockedUsers}";


            // ===================== LOGIN EVENTS =====================
            LoadLoginEventsSafe(connection);

            // Count high-risk login events (failed and suspicious) AFTER loading events
            var highRiskLogins = _allLoginEvents.Count(ev => IsSuspiciousLoginEvent(ev));
            HighRiskUsersText.Text = $"High-risk logins: {highRiskLogins}";

            // ===================== STATUS =====================
            StatusTextBlock.Text =
                $"Database loaded – {users.Count} users";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open database or load data.\n\n{ex.Message}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Učitava login_events ako tabela postoji.
    /// Ako ne postoji – tiho ignoriše (stare baze).
    /// </summary>
    private void LoadLoginEventsSafe(SqliteConnection connection)
    {
        try
        {
            var loginEvents = LoginEventReadRepository.LoadLoginEvents(connection);
            // Mark suspicious events by creating new records if needed
            var processedEvents = loginEvents.Select(ev =>
                IsSuspiciousLoginEvent(ev)
                    ? new LoginEventRecord
                    {
                        Username = ev.Username,
                        Success = ev.Success,
                        Mode = ev.Mode,
                        Reason = "sql_injection_possible",
                        OccurredAt = ev.OccurredAt
                    }
                    : ev
            ).ToList();
            _allLoginEvents = processedEvents;
            LoginEventsDataGrid.ItemsSource = _allLoginEvents;

            // Update high-risk logins KPI after loading events
            var highRiskLogins = _allLoginEvents.Count(ev => IsSuspiciousLoginEvent(ev));
            HighRiskUsersText.Text = $"High-risk logins: {highRiskLogins}";
        }
        catch (SqliteException)
        {
            // Baza nema login_events tabelu (legacy baza)
            _allLoginEvents = new List<LoginEventRecord>();
            LoginEventsDataGrid.ItemsSource = null;
            HighRiskUsersText.Text = "High-risk logins: 0";
        }
    }

    private void FilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var filter = FilterTextBox.Text?.Trim().ToLower() ?? string.Empty;

        // Filter users
        if (_allUsers != null)
        {
            var filteredUsers = string.IsNullOrEmpty(filter)
                ? _allUsers
                : _allUsers.Where(u =>
                    (!string.IsNullOrEmpty(u.Username) && u.Username.ToLower().Contains(filter))
                ).ToList();
            UsersDataGrid.ItemsSource = filteredUsers;
        }

        // Filter login events
        if (_allLoginEvents != null)
        {
            var filteredEvents = string.IsNullOrEmpty(filter)
                ? _allLoginEvents
                : _allLoginEvents.Where(ev =>
                    (!string.IsNullOrEmpty(ev.Username) && ev.Username.ToLower().Contains(filter)) ||
                    (!string.IsNullOrEmpty(ev.Reason) && ev.Reason.ToLower().Contains(filter))
                ).ToList();
            LoginEventsDataGrid.ItemsSource = filteredEvents;

            // Update high-risk logins KPI for filtered view
            var highRiskLogins = filteredEvents.Count(ev => IsSuspiciousLoginEvent(ev));
            HighRiskUsersText.Text = $"High-risk logins: {highRiskLogins}";
        }
    }
}
