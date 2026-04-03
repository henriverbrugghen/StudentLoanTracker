namespace StudentLoanTracker.Core;

/// <summary>One row in an amortization table: payment #, date, split of payment, and balance after.</summary>
public class PaymentScheduleEntry
{
    public int PaymentNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal PaymentAmount { get; set; }
    public decimal PrincipalPortion { get; set; }
    public decimal InterestPortion { get; set; }
    public decimal RemainingBalance { get; set; }
}
