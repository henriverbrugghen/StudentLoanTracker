namespace StudentLoanTracker.Core;

/// <summary>Async CRUD against persisted loans (SQLite implementation in the Data project).</summary>
public interface ILoanRepository
{
    Task<IReadOnlyList<Loan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Loan loan, CancellationToken cancellationToken = default);
    Task UpdateAsync(Loan loan, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
