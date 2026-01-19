MainWindow.xaml - GUI

Dodali smo biblioteku Microsoft.Data.Sqlite kako bismo mogli da koristimo SQLite bazu podataka u aplikaciji:
> dotnet add SecurityDatabaseAnalyzer/SecurityDatabaseAnalyzer.csproj package Microsoft.Data.Sqlite

SQLiteReadOnlyService - otvaranje SQLite baze

MainWindow.xaml.cs - logika

UserReadRepository.cs - čita podatke o korisnicima koje potom prikazujemo na Dashboardu
LoginEventReadRepository.cs - čita activity log