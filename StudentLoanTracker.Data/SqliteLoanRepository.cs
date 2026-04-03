using Microsoft.Data.Sqlite;
using StudentLoanTracker.Core;

namespace StudentLoanTracker.Data;

/// <summary>
/// Maps <see cref="Loan"/> entities to the <c>Loans</c> table using Microsoft.Data.Sqlite.
/// Dates and Guids are stored as ISO strings for portability; decimals map to SQLite REAL.
/// </summary>
public class SqliteLoanRepository : ILoanRepository
{
    private readonly string _connectionString;

    public SqliteLoanRepository(string? connectionString = null)
    {
        _connectionString = connectionString ?? $"Data Source={DatabaseInitializer.GetDatabasePath()}";
    }

    public async Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<Loan>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Servicer, Principal, InterestRate, Compounding, MinimumPayment, StartDate, TermMonths, IsFederal, Notes FROM Loans ORDER BY Name";
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(ReadLoan(reader));
        return list;
    }

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Servicer, Principal, InterestRate, Compounding, MinimumPayment, StartDate, TermMonths, IsFederal, Notes FROM Loans WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;
        return ReadLoan(reader);
    }

    public async Task AddAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("O");
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Loans (Id, Name, Servicer, Principal, InterestRate, Compounding, MinimumPayment, StartDate, TermMonths, IsFederal, Notes, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, @Servicer, @Principal, @InterestRate, @Compounding, @MinimumPayment, @StartDate, @TermMonths, @IsFederal, @Notes, @CreatedAt, @UpdatedAt)
            """;
        AddParameters(cmd, loan, now, now);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.ToString("O");
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Loans SET Name = @Name, Servicer = @Servicer, Principal = @Principal, InterestRate = @InterestRate, Compounding = @Compounding,
            MinimumPayment = @MinimumPayment, StartDate = @StartDate, TermMonths = @TermMonths, IsFederal = @IsFederal, Notes = @Notes, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """;
        cmd.Parameters.AddWithValue("@Id", loan.Id.ToString());
        cmd.Parameters.AddWithValue("@Name", loan.Name);
        cmd.Parameters.AddWithValue("@Servicer", loan.Servicer);
        cmd.Parameters.AddWithValue("@Principal", loan.Principal);
        cmd.Parameters.AddWithValue("@InterestRate", loan.InterestRate);
        cmd.Parameters.AddWithValue("@Compounding", loan.Compounding);
        cmd.Parameters.AddWithValue("@MinimumPayment", loan.MinimumPayment);
        cmd.Parameters.AddWithValue("@StartDate", loan.StartDate.ToString("O"));
        cmd.Parameters.AddWithValue("@TermMonths", loan.TermMonths);
        cmd.Parameters.AddWithValue("@IsFederal", loan.IsFederal ? 1 : 0);
        cmd.Parameters.AddWithValue("@Notes", (object?)loan.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpdatedAt", now);
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Loans WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id.ToString());
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddParameters(SqliteCommand cmd, Loan loan, string createdAt, string updatedAt)
    {
        cmd.Parameters.AddWithValue("@Id", loan.Id.ToString());
        cmd.Parameters.AddWithValue("@Name", loan.Name);
        cmd.Parameters.AddWithValue("@Servicer", loan.Servicer);
        cmd.Parameters.AddWithValue("@Principal", loan.Principal);
        cmd.Parameters.AddWithValue("@InterestRate", loan.InterestRate);
        cmd.Parameters.AddWithValue("@Compounding", loan.Compounding);
        cmd.Parameters.AddWithValue("@MinimumPayment", loan.MinimumPayment);
        cmd.Parameters.AddWithValue("@StartDate", loan.StartDate.ToString("O"));
        cmd.Parameters.AddWithValue("@TermMonths", loan.TermMonths);
        cmd.Parameters.AddWithValue("@IsFederal", loan.IsFederal ? 1 : 0);
        cmd.Parameters.AddWithValue("@Notes", (object?)loan.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", createdAt);
        cmd.Parameters.AddWithValue("@UpdatedAt", updatedAt);
    }

    private static Loan ReadLoan(SqliteDataReader reader)
    {
        return new Loan
        {
            Id = Guid.Parse(reader.GetString(0)),
            Name = reader.GetString(1),
            Servicer = reader.GetString(2),
            Principal = reader.GetDecimal(3),
            InterestRate = reader.GetDecimal(4),
            Compounding = reader.GetString(5),
            MinimumPayment = reader.GetDecimal(6),
            StartDate = DateTime.Parse(reader.GetString(7)),
            TermMonths = reader.GetInt32(8),
            IsFederal = reader.GetInt64(9) != 0,
            Notes = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }
}
