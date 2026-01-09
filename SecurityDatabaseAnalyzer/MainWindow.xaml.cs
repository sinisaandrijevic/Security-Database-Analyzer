using Microsoft.Win32;
using System.IO;
using System.Windows;
using SecurityDatabaseAnalyzer.Database;

namespace SecurityDatabaseAnalyzer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // Ovu metodu poziva dugme "Open Database" iz XAML-a
    private void OpenDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select authentication database",
            Filter = "SQLite database (*.db)|*.db|All files (*.*)|*.*"
        };

        // Ako korisnik klikne Cancel
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            // Otvaramo bazu isključivo u read-only režimu
            using var connection = SQLiteReadOnlyService.Open(dialog.FileName);

            // Ažuriramo status u UI-ju
            StatusTextBlock.Text =
                $"Database loaded (READ-ONLY): {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to open database in read-only mode.\n\n{ex.Message}",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}