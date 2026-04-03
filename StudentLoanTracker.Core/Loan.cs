namespace StudentLoanTracker.Core;

/// <summary>
/// Domain model for one student loan row, mirrored in the SQLite <c>Loans</c> table. The calculator
/// uses principal, annual <see cref="InterestRate"/>, <see cref="TermMonths"/>, and <see cref="StartDate"/>
/// to build schedules; <see cref="MinimumPayment"/> is stored for reference but the schedule uses the
/// level payment derived from principal/rate/term unless you add extra in the UI.
/// </summary>
public class Loan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Servicer { get; set; } = string.Empty;
    public decimal Principal { get; set; }
    public decimal InterestRate { get; set; }
    public string Compounding { get; set; } = "Monthly";
    public decimal MinimumPayment { get; set; }
    public DateTime StartDate { get; set; }
    public int TermMonths { get; set; }
    public bool IsFederal { get; set; }
    public string? Notes { get; set; }
}
