namespace StudentLoanTracker.Core;

/// <summary>Central rules for whether a <see cref="Loan"/> can be saved or passed into the calculator.</summary>
public static class LoanValidator
{
    public const decimal MinRate = 0;
    public const decimal MaxRate = 30;
    public const int MinTermMonths = 1;
    public const int MaxTermMonths = 600;

    /// <summary>Populates <paramref name="errors"/> with human-readable messages; returns <c>true</c> if the list is empty.</summary>
    public static bool IsValid(Loan loan, out IReadOnlyList<string> errors)
    {
        var list = new List<string>();
        if (loan.Principal <= 0) list.Add("Principal must be greater than 0.");
        if (loan.InterestRate < MinRate || loan.InterestRate > MaxRate)
            list.Add($"Interest rate must be between {MinRate} and {MaxRate}.");
        if (loan.TermMonths < MinTermMonths || loan.TermMonths > MaxTermMonths)
            list.Add($"Term must be between {MinTermMonths} and {MaxTermMonths} months.");
        if (loan.MinimumPayment < 0) list.Add("Minimum payment cannot be negative.");
        errors = list;
        return list.Count == 0;
    }
}
