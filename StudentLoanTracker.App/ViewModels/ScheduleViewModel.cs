using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentLoanTracker.Core;
using StudentLoanTracker.App.Services;

namespace StudentLoanTracker.App.ViewModels;

/// <summary>
/// Builds an amortization schedule with <see cref="ILoanCalculator.BuildSchedule"/>. Each
/// <see cref="PaymentScheduleEntry"/> is one payment period. Changing <see cref="ExtraMonthlyPayment"/>
/// recalculates the whole schedule (same payment engine as the charts screen).
/// </summary>
public partial class ScheduleViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly ILoanCalculator _loanCalculator;
    private readonly Loan _loan;

    [ObservableProperty]
    private decimal _extraMonthlyPayment;

    [ObservableProperty]
    private string _loanName = string.Empty;

    public ObservableCollection<PaymentScheduleEntry> Schedule { get; } = new();
    public decimal TotalInterest { get; private set; }
    public decimal TotalPrincipal { get; private set; }
    public int PayoffMonths { get; private set; }
    public DateTime? PayoffDate { get; private set; }

    public ScheduleViewModel(INavigationService navigation, ILoanCalculator loanCalculator, Loan loan)
    {
        _navigation = navigation;
        _loanCalculator = loanCalculator;
        _loan = loan;
        LoanName = loan.Name;
        RefreshSchedule();
    }

    [RelayCommand]
    private void Back()
    {
        _navigation.NavigateToLoanList();
    }

    partial void OnExtraMonthlyPaymentChanged(decimal value) => RefreshSchedule();

    /// <summary>Recomputes rows and summary totals from the calculator and notifies bound properties.</summary>
    private void RefreshSchedule()
    {
        Schedule.Clear();
        var entries = _loanCalculator.BuildSchedule(_loan, ExtraMonthlyPayment > 0 ? ExtraMonthlyPayment : null).ToList();
        foreach (var e in entries)
            Schedule.Add(e);
        TotalInterest = entries.Sum(e => e.InterestPortion);
        TotalPrincipal = entries.Sum(e => e.PrincipalPortion);
        PayoffMonths = entries.Count;
        PayoffDate = entries.Count > 0 ? entries[^1].Date : null;
        OnPropertyChanged(nameof(TotalInterest));
        OnPropertyChanged(nameof(TotalPrincipal));
        OnPropertyChanged(nameof(PayoffMonths));
        OnPropertyChanged(nameof(PayoffDate));
    }
}
