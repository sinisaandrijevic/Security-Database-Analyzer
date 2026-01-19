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
            // 1. Otvori bazu ISKLJUČIVO u read-only režimu
            using var connection = SQLiteReadOnlyService.Open(dialog.FileName);

            // ===================== USERS =====================
            var users = UserReadRepository.LoadUsers(connection);
            _allUsers = users.ToList();
            UsersDataGrid.ItemsSource = _allUsers;

            // ===================== SECURITY INSIGHTS =====================
            TotalUsersText.Text = $"Total users: {users.Count}";

            var lockedUsers = users.Count(u => u.Locked);
            LockedUsersText.Text = $"Locked users: {lockedUsers}";

            var highRiskUsers = users.Count(u => u.FailedAttempts >= 5);
            HighRiskUsersText.Text = $"High-risk users: {highRiskUsers}";

            // ===================== LOGIN EVENTS =====================
            LoadLoginEventsSafe(connection);

            // ===================== STATUS =====================
            StatusTextBlock.Text =
                $"Database loaded (READ-ONLY) – {users.Count} users";
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
            _allLoginEvents = loginEvents.ToList();
            LoginEventsDataGrid.ItemsSource = _allLoginEvents;
        }
        catch (SqliteException)
        {
            // Baza nema login_events tabelu (legacy baza)
            _allLoginEvents = new List<LoginEventRecord>();
            LoginEventsDataGrid.ItemsSource = null;
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
        }
    }
}
