namespace StudentLoanTracker.Core;

/// <summary>
/// Standard fixed-payment amortization: each month, interest accrues on the remaining balance at
/// <c>annual rate / 12</c>, then the rest of the payment reduces principal. The base payment is the
/// level payment that would retire the loan in <see cref="Loan.TermMonths"/> (same formula as a mortgage).
/// Optional <c>extraMonthlyPayment</c> is added on top of that payment to shorten the schedule.
/// </summary>
public class LoanCalculator : ILoanCalculator
{
    /// <summary>Yields one <see cref="PaymentScheduleEntry"/> per month until the balance is ~zero or a safety cap (600) is hit.</summary>
    public IEnumerable<PaymentScheduleEntry> BuildSchedule(Loan loan, decimal? extraMonthlyPayment = null)
    {
        if (!LoanValidator.IsValid(loan, out _))
            yield break;

        decimal extra = extraMonthlyPayment ?? 0;
        decimal monthlyRate = loan.InterestRate / 100m / 12m;
        decimal balance = loan.Principal;
        decimal totalPayment = GetLevelPayment(loan) + extra;
        DateTime date = loan.StartDate;
        int paymentNumber = 0;

        while (balance > 0.01m && paymentNumber < 600)
        {
            paymentNumber++;
            decimal interestPortion = balance * monthlyRate;
            decimal principalPortion = Math.Min(totalPayment - interestPortion, balance);
            decimal paymentAmount = principalPortion + interestPortion;
            balance = Math.Max(0, balance - principalPortion);

            yield return new PaymentScheduleEntry
            {
                PaymentNumber = paymentNumber,
                Date = date,
                PaymentAmount = paymentAmount,
                PrincipalPortion = principalPortion,
                InterestPortion = interestPortion,
                RemainingBalance = balance
            };

            date = date.AddMonths(1);
        }
    }

    public decimal GetTotalInterest(Loan loan, decimal? extraMonthlyPayment = null)
    {
        return BuildSchedule(loan, extraMonthlyPayment).Sum(e => e.InterestPortion);
    }

    public DateTime? GetPayoffDate(Loan loan, decimal? extraMonthlyPayment = null)
    {
        var last = BuildSchedule(loan, extraMonthlyPayment).LastOrDefault();
        return last != null ? last.Date : null;
    }

    public int GetPayoffMonths(Loan loan, decimal? extraMonthlyPayment = null)
    {
        return BuildSchedule(loan, extraMonthlyPayment).Count();
    }

    /// <summary>Monthly payment for an amortizing loan (PMT); if rate is 0, principal is divided evenly by term.</summary>
    private static decimal GetLevelPayment(Loan loan)
    {
        if (loan.Principal <= 0 || loan.TermMonths <= 0) return 0;
        decimal monthlyRate = loan.InterestRate / 100m / 12m;
        if (monthlyRate == 0)
            return loan.Principal / loan.TermMonths;
        return loan.Principal * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, loan.TermMonths))
            / ((decimal)Math.Pow(1 + (double)monthlyRate, loan.TermMonths) - 1);
    }
}
