using Microsoft.Data.Sqlite;

namespace StudentLoanTracker.Data;

/// <summary>
/// Ensures the app data folder and <c>Loans</c> table exist. The database file lives under
/// <c>%AppData%\StudentLoanTracker\student_loans.db</c> so it survives app updates and is per-user on Windows.
/// </summary>
public static class DatabaseInitializer
{
    private const string CreateTableSql = """
        CREATE TABLE IF NOT EXISTS Loans (
            Id TEXT PRIMARY KEY,
            Name TEXT NOT NULL,
            Servicer TEXT NOT NULL,
            Principal REAL NOT NULL,
            InterestRate REAL NOT NULL,
            Compounding TEXT NOT NULL,
            MinimumPayment REAL NOT NULL,
            StartDate TEXT NOT NULL,
            TermMonths INTEGER NOT NULL,
            IsFederal INTEGER NOT NULL,
            Notes TEXT,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL
        );
        """;

    public static string GetDatabasePath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StudentLoanTracker");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "student_loans.db");
    }

    /// <summary>Creates the database file on disk and applies the schema if the table is missing.</summary>
    public static void Initialize()
    {
        var path = GetDatabasePath();
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = CreateTableSql;
        cmd.ExecuteNonQuery();
    }
}
