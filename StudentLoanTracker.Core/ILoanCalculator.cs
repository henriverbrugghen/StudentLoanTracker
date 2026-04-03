namespace StudentLoanTracker.Core;

/// <summary>Amortization and payoff projections for a <see cref="Loan"/>.</summary>
public interface ILoanCalculator
{
    IEnumerable<PaymentScheduleEntry> BuildSchedule(Loan loan, decimal? extraMonthlyPayment = null);
    decimal GetTotalInterest(Loan loan, decimal? extraMonthlyPayment = null);
    DateTime? GetPayoffDate(Loan loan, decimal? extraMonthlyPayment = null);
    int GetPayoffMonths(Loan loan, decimal? extraMonthlyPayment = null);
}
